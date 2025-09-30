using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Permission.Api.Dtos;
using Permission.Api.Entities;
using SharedLibrary.Dtos.Common;
using SharedLibrary.Dtos.PermissionDtos;
using SharedLibrary.Exceptions;
using SharedLibrary.HelperServices;
using SharedLibrary.Requests;
using SharedLibrary.StaticDatas;

namespace Permission.Api.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IMongoCollection<UserPermission> _userPermissions;
        private readonly IMongoCollection<Page> _pages;
        private readonly CurrentUser _currentUser;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PermissionService(MongoDbService mongoDbService,
            CurrentUser currentUser,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            var database = mongoDbService.GetDatabase();
            _pages = database.GetCollection<Page>("Pages");
            _userPermissions = database.GetCollection<UserPermission>("UserPermissions");
            _currentUser = currentUser;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        /// <summary>
        /// currentUser-in icazesi olan butun sehife ve action-larini getirir
        /// </summary>
        public async Task<List<PageDto>> GetAllPagesAndActionsAsync()
        {
            var filter = Builders<UserPermission>.Filter.Eq(up => up.UserId, _currentUser.UserGuid);
            var userPermissions = await _userPermissions.Find(filter).FirstOrDefaultAsync();

            if (userPermissions == null)
            {
                return [];
            }

            var allPages = await _pages.Find(_ => true).ToListAsync();
            var pageDict = allPages.ToDictionary(p => p.Key);

            var authorizedPages = userPermissions.Permissions
                            .Where(p => pageDict.ContainsKey(p.PageKey))
                            .Select(p => new PageDto
                            {
                                Key = p.PageKey,
                                Name = pageDict[p.PageKey].Name,
                                Actions = p.ActionKeys
                                    .Where(actionKey => pageDict[p.PageKey]
                                                             .Actions.Any(a => a.Key == actionKey))
                                    .Select(actionKey => new ActionDto
                                    {
                                        Key = actionKey,
                                        Name = pageDict[p.PageKey].Actions.First(a => a.Key == actionKey).Name
                                    }).ToList()
                            })
                           .ToList();

            return authorizedPages;
        }

        /// <summary>
        /// butun selahiyyet siyahisi post metodu, front-dan ancaq deyisiklik olan user-in icazeleri gelecek
        /// </summary>
        public async Task<bool> UpdateUserPermissionsAsync(List<UpdateUserPermissionsDto> updatedPermissions)
        {
            // TODO: ozunun ve superadminin icazelerini deyise bilmemelidir(getde gormur)
            var bulkOps = new List<WriteModel<UserPermission>>();

            // currentUser-in movcud icazelerini getir
            var currentUserPermissions = await _userPermissions
                                                   .Find(up => up.UserId == _currentUser.UserGuid)
                                                   .FirstOrDefaultAsync() ?? throw new NotFoundException("İstifadəçi mövcud deyil.");

            // db-de movcud olan butun page ve action-lari getir
            var existingPages = await _pages
                                         .Find(_ => true)
                                         .Project(p => new { p.Key, p.Name, Actions = p.Actions.ToList() })
                                         .ToListAsync();

            var existingPageDict = existingPages
                                         .ToDictionary(p => p.Key, p => p.Actions.Select(a => a.Key).ToHashSet());

            // currentUser-in deyise biləcəyi səhifələr
            var currentPermissionDict = currentUserPermissions.Permissions
                                          .ToDictionary(p => p.PageKey, p => p.ActionKeys.ToHashSet());

            foreach (var updatedPermission in updatedPermissions)
            {
                var validatedPermissions = updatedPermission.Permissions
                                             .Where(p => p.Actions.Any())
                                             .Select(p => new UserPagePermission
                                             {
                                                 PageKey = p.Key,
                                                 ActionKeys = p.Actions.ToList()
                                             }).ToList();

                ValidatePermissionsOrThrow(validatedPermissions, currentPermissionDict, existingPageDict);

                var filter = Builders<UserPermission>.Filter.Eq(up => up.UserId, updatedPermission.UserId);
                var update = Builders<UserPermission>.Update.Set(up => up.Permissions, validatedPermissions);

                bulkOps.Add(new UpdateOneModel<UserPermission>(filter, update) { IsUpsert = true });
            }

            if (bulkOps.Count != 0)
            {
                var result = await _userPermissions.BulkWriteAsync(bulkOps);
                return result.IsAcknowledged;
            }

            return false;
        }

        private void ValidatePermissionsOrThrow(List<UserPagePermission> permissionsToSet,
                                                 Dictionary<string, HashSet<string>> currentUserPermissions,
                                                  Dictionary<string, HashSet<string>> existingPages)
        {
            foreach (var page in permissionsToSet)
            {
                if (!existingPages.ContainsKey(page.PageKey))
                    throw new NotFoundException("Səhifə mövcud deyil.");

                if (!currentUserPermissions.ContainsKey(page.PageKey))
                    //throw new ForbiddenException("Səhifə üçün icazəniz yoxdur.");
                    throw new BadRequestException("Səhifə üçün icazəniz yoxdur.");

                foreach (var action in page.ActionKeys)
                {
                    if (!existingPages[page.PageKey].Contains(action))
                        throw new NotFoundException("Əməliyyat mövcud deyil.");

                    if (!currentUserPermissions[page.PageKey].Contains(action))
                        //throw new ForbiddenException("Əməliyyat üçün icazəniz yoxdur.");
                        throw new BadRequestException("Əməliyyat üçün icazəniz yoxdur.");
                }
            }
        }

        /// <summary>
        /// permission siyahisinin get-i, groupId null gonderilerse parent qrup gelecek, 
        /// id gonderilerse o qrupun alt qruplari ve birbasa o qrupa bagli userler 
        /// </summary>
        public async Task<DataListDto<UserPermissionsDto>> GetAllUserPermissions(int skip = 0, int take = 10)
        {
            var currentUser = await _userPermissions.Find(x => x.UserId == _currentUser.UserGuid)
                                   .FirstOrDefaultAsync() ?? throw new NotFoundException("İstifadəçi mövcud deyil.");
            var currentPermissionDict = currentUser.Permissions
                .ToDictionary(p => p.PageKey, p => p.ActionKeys.ToHashSet());

            Guid superAdminGuid = Guid.Parse(SuperAdminData.Id);

            var baseQuery = _userPermissions.AsQueryable()
                .Where(u => u.UserId != superAdminGuid &&
                             u.UserId != _currentUser.UserGuid);

            var userPermissions = await baseQuery
                .Skip(skip * take)
                .Take(take)
                .ToListAsync();

            var users = userPermissions.Select(u =>
            {
                var permissions = u.Permissions
                    .Where(p => currentPermissionDict.ContainsKey(p.PageKey))
                    .Select(p => new UserPermissionPageDto
                    {
                        PageKey = p.PageKey,
                        ActionKeys = p.ActionKeys.Where(a => currentPermissionDict[p.PageKey].Contains(a)).ToList()
                    })
                    .Where(p => p.ActionKeys.Any())
                    .ToList();

                return new UserPermissionsDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Permissions = permissions
                };
            })
              .ToList();

            return new DataListDto<UserPermissionsDto>
            {
                Datas = users,
                TotalCount = await baseQuery.CountAsync()
            };
        }

        /// <summary>
        /// Verilmiş istifadəçiyə aid permission siyahısını qaytarır.
        /// </summary>
        public async Task<UserPermissionsDto> GetUserPermissionsById(Guid userId)
        {
            var currentUser = await _userPermissions.Find(x => x.UserId == _currentUser.UserGuid)
                                       .FirstOrDefaultAsync() ?? throw new NotFoundException("Hazırkı istifadəçi mövcud deyil.");
            if (userId == _currentUser.UserGuid)
                throw new BadRequestException("Öz icazələrinizi bu metoddan əldə edə bilməzsiniz.");

            Guid superAdminGuid = Guid.Parse(SuperAdminData.Id);
            if (userId == superAdminGuid)
                throw new NotFoundException("Super admin üçün icazə məlumatları göstərilə bilməz.");

            var targetUser = await _userPermissions.Find(x => x.UserId == userId)
                                                   .FirstOrDefaultAsync() ?? throw new NotFoundException("İstifadəçi tapılmadı.");

            var currentPermissionDict = currentUser.Permissions
                .ToDictionary(p => p.PageKey, p => p.ActionKeys.ToHashSet());

            var permissions = targetUser.Permissions
                .Where(p => currentPermissionDict.ContainsKey(p.PageKey))
                .Select(p => new UserPermissionPageDto
                {
                    PageKey = p.PageKey,
                    ActionKeys = p.ActionKeys
                                 .Where(a => currentPermissionDict[p.PageKey].Contains(a))
                                 .ToList()
                })
                .Where(p => p.ActionKeys.Any())
                .ToList();

            return new UserPermissionsDto
            {
                UserId = targetUser.UserId,
                FullName = targetUser.FullName,
                Permissions = permissions
            };
        }

        /// <summary>
        /// currentUser-ın icazelerini qaytarir
        /// </summary>
        public async Task<UserPermissionsDto?> GetCurrentUserPermissionsAsync()
        {
            var currentUserPermissions = await _userPermissions
                .Find(up => up.UserId == _currentUser.UserGuid)
                .FirstOrDefaultAsync();

            if (currentUserPermissions == null)
            {
                return null;
            }

            return new UserPermissionsDto
            {
                UserId = _currentUser.UserGuid,
                FullName = currentUserPermissions.FullName,
                Permissions = currentUserPermissions.Permissions
                                     .Select(p => new UserPermissionPageDto
                                     {
                                         PageKey = p.PageKey,
                                         ActionKeys = p.ActionKeys
                                     })
                                     .ToList()
            };
        }

        /// <summary>
        /// user-in permissionu olub olmamasinin yoxlanilmasi
        /// </summary>
        public async Task<bool> CheckPermissionAsync(Guid userId, string page, string action)
        {
            var filter = Builders<UserPermission>.Filter.Eq(up => up.UserId, userId);
            var userPermission = await _userPermissions.Find(filter).FirstOrDefaultAsync();

            if (userPermission == null) return false;

            return userPermission.Permissions.Any(p =>
                p.PageKey == page &&
                p.ActionKeys.Contains(action)
            );
        }

        #region Diger service-lerle elaqe 

        /// <summary>
        /// main proyektden user-leri cekib UserPermissiona insert etmek(servis ilk qosuldugunda)
        /// </summary>
        public async Task SyncUsersAsync()
        {
            var url = $"{_configuration["AuthService:BaseUrl"]}/Auth/GetAllUsersForPermission";
            var response = await _httpClient.GetAsync(url);
            var mainData = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            if (mainData == null || !mainData.Any()) return;

            await SyncUsersAsync(mainData);
        }

        /// <summary>
        /// userlerin sync olunmasi  
        /// </summary>
        private async Task SyncUsersAsync(List<UserDto> users)
        {
            List<UserPermission> existingUsers;
            try
            {
                existingUsers = await _userPermissions.Find(_ => true).ToListAsync();
            }
            catch
            {
                existingUsers = [];
            }

            var existingUserDict = existingUsers.ToDictionary(u => u.UserId);
            var incomingIds = users.Select(u => u.UserId).ToHashSet();

            var newUsers = users
                .Where(u => !existingUserDict.ContainsKey(u.UserId))
                .Select(u => new UserPermission
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    FullName = u.FullName,
                    IsBlocked = u.IsBlocked,
                    UserId = u.UserId,
                    Permissions = new List<UserPagePermission>()
                }).ToList();

            var updatedUsers = users
                .Where(u => existingUserDict.TryGetValue(u.UserId, out var existing)
                    && (existing.FullName != u.FullName || existing.IsBlocked != u.IsBlocked))
                .Select(u => new UpdateOneModel<UserPermission>(
                    Builders<UserPermission>.Filter.Eq(x => x.UserId, u.UserId),
                    Builders<UserPermission>.Update
                        .Set(x => x.FullName, u.FullName)
                        .Set(x => x.IsBlocked, u.IsBlocked)
                )).ToList();

            var deletedUserIds = existingUserDict.Keys.Except(incomingIds).ToList();

            if (newUsers.Count != 0) await _userPermissions.InsertManyAsync(newUsers);
            if (updatedUsers.Count != 0) await _userPermissions.BulkWriteAsync(updatedUsers);
            if (deletedUserIds.Count != 0) await _userPermissions.DeleteManyAsync(u => deletedUserIds.Contains(u.UserId));
        }

        /// <summary>
        /// proyekt ayaga qalxanda attributlardan oxunan page ve action-larin db-e elave olunmasi
        /// </summary>
        public async Task<List<string>> SyncPagesAndActionsAsync(PermissionRegisteredRequest permissionData)
        {
            // butun deyisiklikleri sonda gostermek ucun 
            var logMessages = new List<string>();

            // var olan sehifeleri aliriq
            var existingPages = await _pages.Find(_ => true).ToListAsync();
            var existingPageDict = existingPages.ToDictionary(p => p.Key, p => p);

            // attributlardan oxunan datalari page adlarina gore qruplayiriq 
            var groupedPages = permissionData.Pages
                                 .GroupBy(p => new { p.Key, p.Name })
                                 .Select(g => new Page
                                 {
                                     Key = g.Key.Key,
                                     Name = g.Key.Name,
                                     Actions = g.SelectMany(p => p.Actions)
                                                .GroupBy(a => new { a.Key, a.Name })
                                                .Select(a => new Entities.Action
                                                {
                                                    Key = a.Key.Key,
                                                    Name = a.Key.Name
                                                }).ToList()
                                 })
                                 .ToList();


            var foundPageKeys = groupedPages.Select(p => p.Key).ToHashSet();
            var newPages = new List<Page>();

            foreach (var page in groupedPages)
            {
                if (!existingPageDict.TryGetValue(page.Key, out var existingPage))
                {
                    newPages.Add(page);
                    continue;
                }

                // səhifə adı dəyişibsə yenile
                if (existingPage.Name != page.Name)
                {
                    var nameUpdate = Builders<Page>.Update.Set(p => p.Name, page.Name);
                    await _pages.UpdateOneAsync(p => p.Key == page.Key, nameUpdate);
                    logMessages.Add($"'{existingPage.Name}' səhifəsinin adı '{page.Name}' olaraq dəyişdirildi.");
                }

                // yeni action-lar
                var existingActionDict = existingPage.Actions.ToDictionary(a => a.Key, a => a);

                var newActions = page.Actions
                    .Where(a => !existingActionDict.ContainsKey(a.Key))
                    .ToList();

                if (newActions.Count != 0)
                {
                    var actionAddUpdate = Builders<Page>.Update.AddToSetEach(p => p.Actions, newActions);
                    await _pages.UpdateOneAsync(p => p.Key == page.Key, actionAddUpdate);

                    var addedActionNames = newActions.Select(a => $"'{a.Name}'").ToList();
                    var combinedActions = string.Join(", ", addedActionNames);
                    logMessages.Add($"'{page.Name}' səhifəsinə {combinedActions} əməliyyat(lar)ı əlavə olundu.");
                }

                // mövcud action-un adı dəyişibsə yenile
                foreach (var updatedAction in page.Actions)
                {
                    var keyLower = updatedAction.Key.ToLowerInvariant();
                    if (existingActionDict.TryGetValue(keyLower, out var existingAction) &&
                        existingAction.Name != updatedAction.Name)
                    {
                        var actionUpdate = Builders<Page>.Update
                            .Set("actions.$[elem].name", updatedAction.Name);

                        var arrayFilters = new List<ArrayFilterDefinition>
                                 {
                                  new JsonArrayFilterDefinition<Entities.Action>("{ 'elem.key': '" + updatedAction.Key + "' }")
                                 };

                        var options = new UpdateOptions { ArrayFilters = arrayFilters };

                        await _pages.UpdateOneAsync(
                            Builders<Page>.Filter.Eq(p => p.Key, page.Key),
                            actionUpdate,
                            options);
                        logMessages.Add($"'{page.Name}' səhifəsində '{existingAction.Name}' əməliyyatının adı '{updatedAction.Name}' olaraq dəyişdirildi.");
                    }
                }
            }

            if (newPages.Count != 0)
            {
                await _pages.InsertManyAsync(newPages);

                foreach (var page in newPages)
                {
                    var actionNames = page.Actions.Select(a => $"'{a.Name}'").ToList();
                    var actionsCombined = actionNames.Any()
                        ? $" Əməliyyatlar: {string.Join(", ", actionNames)}"
                        : string.Empty;

                    logMessages.Add($"'{page.Name}' adlı yeni səhifə əlavə olundu.{actionsCombined}");
                }

            }

            // Super admin permission-lari əlavə et
            await AddPermissionsToAdminAsync(groupedPages.Select(p => new UserPagePermission
            {
                PageKey = p.Key,
                ActionKeys = p.Actions.Select(a => a.Key).ToList()
            }).ToList());

            // db-de olub yeni oxunan attributlar arasinda olmayan page ve action varsa onu silirik 
            logMessages.AddRange(await RemoveDeletedPagesAndActionsAsync(existingPages, groupedPages));
            return logMessages;
        }

        private async Task AddPermissionsToAdminAsync(List<UserPagePermission> allPermissions)
        {
            string superAdminUserId = SuperAdminData.Id;
            var filter = Builders<UserPermission>.Filter.Eq(up => up.UserId, Guid.Parse(superAdminUserId));

            var existing = await _userPermissions.Find(filter).FirstOrDefaultAsync();

            var newPermissions = allPermissions
                                      .Select(p => new UserPagePermission
                                      {
                                          PageKey = p.PageKey,
                                          ActionKeys = p.ActionKeys
                                      })
                                      .ToList();

            if (existing != null)
            {
                // superadmin varsa icazeleri yenile 
                var update = Builders<UserPermission>.Update.Set(up => up.Permissions, newPermissions);
                await _userPermissions.UpdateOneAsync(filter, update);
            }
            else
            {
                var userPermission = new UserPermission
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = Guid.Parse(superAdminUserId),
                    FullName = "Super Admin",
                    IsBlocked = false,
                    Permissions = newPermissions
                };

                await _userPermissions.InsertOneAsync(userPermission);
            }
        }

        private async Task<List<string>> RemoveDeletedPagesAndActionsAsync(List<Page> existingPages, List<Page> newPages)
        {
            var logMessages = new List<string>();

            var newPageKeySet = newPages.Select(p => p.Key).ToHashSet();

            // olmayan sehifeleri sil
            var pagesToDelete = existingPages.Where(p => !newPageKeySet.Contains(p.Key)).ToList();

            if (pagesToDelete.Any())
            {
                var deletedKeys = pagesToDelete.Select(p => p.Key).ToList();

                var deletedNames = pagesToDelete.Select(p => $"'{p.Name}'");
                logMessages.Add($"{string.Join(", ", deletedNames)} səhifə(ləri) silindi.");

                await Task.WhenAll(
                    _pages.DeleteManyAsync(p => deletedKeys.Contains(p.Key)),
                    _userPermissions.UpdateManyAsync(
                        FilterDefinition<UserPermission>.Empty,
                        Builders<UserPermission>.Update.PullFilter(
                            u => u.Permissions,
                            p => deletedKeys.Contains(p.PageKey)
                        )
                    )
                );
            }

            // qalan sehifelerin actionlarina bax
            var pagesToUpdate = existingPages.Where(p => newPageKeySet.Contains(p.Key)).ToList();

            foreach (var oldPage in pagesToUpdate)
            {
                var newPage = newPages.FirstOrDefault(p => p.Key == oldPage.Key);
                if (newPage == null) continue;

                var oldActions = oldPage.Actions.Select(a => a.Key).ToList();
                var newActions = newPage.Actions.Select(a => a.Key).ToList();

                var deletedActions = oldActions.Except(newActions).ToList();

                // esl silinəcək orijinal action key-ləri tapılır
                var deletedOriginalActions = oldPage.Actions
                    .Where(a => deletedActions.Contains(a.Key))
                    .ToList();

                if (deletedOriginalActions.Count != 0)
                {
                    var pullUpdate = Builders<Page>.Update.PullFilter(
                                                p => p.Actions,
                                                Builders<Entities.Action>.Filter.In(a => a.Key, deletedActions)
                                            );

                    await _pages.UpdateOneAsync(p => p.Key == oldPage.Key, pullUpdate);

                    //var userUpdate = Builders<UserPermission>.Update.PullFilter(
                    //    up => up.Permissions[-1].Actions,
                    //    actionKey => deletedActions.Contains(actionKey)
                    //);

                    // `$` positional operator 
                    var filter = Builders<UserPermission>.Filter
                                           .ElemMatch(up => up.Permissions, p => p.PageKey == oldPage.Key);

                    var update = new BsonDocument("$pull", new BsonDocument("permissions.$.actions",
                                                    new BsonDocument("$in", new BsonArray(deletedOriginalActions.Select(a => a.Key)))));

                    await _userPermissions.UpdateManyAsync(filter, update);

                    var actionNames = deletedOriginalActions.Select(a => $"'{a.Name}'").ToList();
                    var combined = string.Join(", ", actionNames);
                    logMessages.Add($"'{oldPage.Name}' səhifəsindən {combined} əməliyyatı(ları) silindi.");
                }
            }

            return logMessages;
        }

        #endregion

        #region Consumer metodlari

        /// <summary>
        /// user-qeydiyyatdan kecende adini db-ye artiririq, permission-lari bos olur
        /// </summary>
        public async Task<bool> AddUserPermissionAsync(UserPermission userPermission)
        {
            var filter = Builders<UserPermission>.Filter.Eq(u => u.UserId, userPermission.UserId);
            var existingUser = await _userPermissions.Find(filter).FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return false; // varsa elave etme
            }

            await _userPermissions.InsertOneAsync(userPermission);
            return true;
        }

        /// <summary>
        /// user-in statusunu(aktiv/deaktiv) deyismek 
        /// </summary>
        //public async Task<bool> ChangeUserStatusAsync(string userId, bool isBlocked)
        //{
        //    var filter = Builders<UserPermission>.Filter.Eq(u => u.UserId, userId);
        //    var update = Builders<UserPermission>.Update.Set(u => u.IsBlocked, isBlocked);

        //    var result = await _userPermissions.UpdateOneAsync(filter, update);

        //    return result.ModifiedCount > 0;
        //}

        /// <summary>
        /// user-in FullName-i update olarsa mongo-daki FullName-ni de update edirik 
        /// </summary>
        //public async Task<bool> UpdateUserFullNameAsync(string userId, string fullName)
        //{
        //    var filter = Builders<UserPermission>.Filter.Eq(u => u.UserId, userId);
        //    var update = Builders<UserPermission>.Update.Set(u => u.FullName, fullName);

        //    var result = await _userPermissions.UpdateOneAsync(filter, update);

        //    return result.ModifiedCount > 0;
        //}

        #endregion
    }
}