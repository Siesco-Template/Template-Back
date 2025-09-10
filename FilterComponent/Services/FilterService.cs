using FilterComponent.Dtos;
using FilterComponent.Entities;
using FilterComponent.Extensions;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using SharedLibrary.Exceptions;
using SharedLibrary.HelperServices;
using QueryGenerator.Core;

namespace FilterComponent.Services
{
    public class FilterService
    {
        private readonly IMongoCollection<Filter> _filtersCollection;
        private readonly CurrentUser _currentUser;
        public FilterService(MongoDbService _mongoDbService, CurrentUser currentUser)
        {
            var database = _mongoDbService.GetDatabase();
            _filtersCollection = database.GetCollection<Filter>("Filters");
            _currentUser = currentUser;
        }


        /// <summary>
        /// Filteri yaratmaq 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task SaveFilter(CreateFilterDto dto)
        {
            if (_currentUser.UserId == null) throw new Exception("Istifadeci tapilmadi");
            var query = IQueryableExtensions.GenerateQuery(dto.FilterValues);

            await _filtersCollection.InsertOneAsync(new Filter
            {
                CreatedAt = DateTime.Now,
                IsDefault = false,
                FilterTitle = dto.FilterTitle.Trim(),
                FilterQuery = query,
                TableId = dto.TableId,
                FilterValues = dto.FilterValues,
                UserId = _currentUser.UserId
            });
        }

        /// <summary>
        /// Filteri silmək
        /// </summary>
        /// <param name="filterId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DeleteFilter(string filterId)
        {
            var filter = await GetFilterById(filterId);
            if (filter == null || filter.UserId != _currentUser.UserId) throw new Exception("Filter mövcud deyil");

            await _filtersCollection.DeleteOneAsync(x => x.Id == filterId);
        }

        /// <summary>
        /// Filteri update edir(Başlıq və filterin tərkibini)
        /// </summary>
        /// <param name="filterId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdateFilter(string filterId, UpdateFilterDto dto)
        {
            //if (!await CheckFilterIsExist(filterId)) throw new Exception("File mövcud deyil");
            var filter = await GetFilterById(filterId);
            if (filter == null || filter.UserId != _currentUser.UserId) throw new NotFoundException("Filter mövcud deyil");

            UpdateDefinition<Filter> updateFilterDefinition = Builders<Filter>.Update
                                                                                    .Set(x => x.FilterTitle, dto.FilterTitle.Trim())
                                                                                    .Set(x => x.FilterValues, dto.FilterValues)
                                                                                    .Set(x => x.FilterQuery, IQueryableExtensions.GenerateQuery(dto.FilterValues));

            await _filtersCollection.UpdateOneAsync(x => x.Id == filterId, updateFilterDefinition);
        }

        /// <summary>
        /// Id-ə uyğun filteri həmin filterin aid olduğu table-in default filteri edir
        /// </summary>
        /// <param name="filterId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task SetDefault(string filterId)
        {
            var filter = await GetFilterById(filterId);
            if (filter == null || filter.UserId != _currentUser.UserId) throw new Exception("Filter mövcud deyil");

            var oldDefaultFilter = await GetDefaultFilter(filter.TableId);
            if (oldDefaultFilter != null && oldDefaultFilter.Id != filterId)
            {
                var updateOldFilterDefinition = Builders<Filter>.Update.Set(x => x.IsDefault, false);
                await _filtersCollection.UpdateOneAsync(x => x.Id == oldDefaultFilter.Id, updateOldFilterDefinition);
            }

            var updateNewFilterDefinition = Builders<Filter>.Update.Set(x => x.IsDefault, true);
            await _filtersCollection.UpdateOneAsync(x => x.Id == filterId, updateNewFilterDefinition);
        }

        /// <summary>
        /// Id-ə uyğun filteri həmin filterin aid olduğu table-in default filterindən çıxardır
        /// </summary>
        /// <param name="filterId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task RemoveDefault(string filterId)
        {
            var filter = await GetFilterById(filterId);

            if (filter is null || filter.UserId != _currentUser.UserId) throw new Exception("Filter mövcud deyil");
            if (filter.IsDefault == false) throw new Exception("Bu filter default deyil");

            var updateDefaultFilterDefinition = Builders<Filter>.Update.Set(x => x.IsDefault, false);
            await _filtersCollection.UpdateOneAsync(x => x.Id == filterId, updateDefaultFilterDefinition);
        }

        /// <summary>
        /// Table-in bütün filterlərini gətirir
        /// </summary>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public async Task<List<FilterListDto>> GetAllFilters(string tableId)
        {
            var filters = await _filtersCollection.Find(x => x.TableId == tableId && x.UserId == _currentUser.UserId)
                .SortByDescending(x => x.FilterTitle)
                .Project(x => new FilterListDto
                {
                    Id = x.Id,
                    FilterTitle = x.FilterTitle,
                    IsDefault = x.IsDefault,
                    FilterValues = x.FilterValues
                })
                .ToListAsync();

            return filters;
        }

        /// <summary>
        /// Id-ə uyğun table-in default filterini gətirir
        /// </summary>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public async Task<Filter> GetDefaultFilter(string tableId)
        {
            var defaultFilter = await _filtersCollection.Find(filter => filter.IsDefault && filter.TableId == tableId && filter.UserId == _currentUser.UserId).FirstOrDefaultAsync();
            return defaultFilter;
        }

        /// <summary>
        /// Id-ə uyğun table-in default filterin sorğusunu gətirir
        /// </summary>
        /// <param name="tableId"></param>
        /// <returns></returns>
        public async Task<string?> GetDefaultFilterQuery(string tableId)
        {
            var defaultFilter = await _filtersCollection
                .Find(filter => filter.IsDefault && filter.TableId == tableId && filter.UserId == _currentUser.UserId)
                .Project(x => x.FilterQuery)
                .FirstOrDefaultAsync();

            return defaultFilter;
        }

        /// <summary>
        /// Id-ə uyğun filteri gətirir
        /// </summary>
        /// <param name="filterId"></param>
        /// <returns></returns>
        public async Task<Filter> GetFilterById(string filterId)
        {
            return await _filtersCollection.Find(x => x.Id == filterId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Əsas metod budur və demək olar ki bütün endpointlərdə bu istifadə ediləcək
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">Üzərində filter edilməsi istənilən query</param>
        /// <param name="tableId">Filter aparılan table</param>
        /// <param name="filters">Spesifik filterlər</param>
        /// <returns></returns>
        public async Task<IQueryable<T>> ApplyFilter<T>(IQueryable<T> query, FilterDto filterDto)
        {
            if (filterDto.Filters != null && filterDto.Filters.Any())
            {
                query = query.Where(IQueryableExtensions.GenerateQuery(filterDto.Filters)!);
            }
            else if(filterDto.TableId != null)
            {
                string? filter = await GetDefaultFilterQuery(filterDto.TableId);
                if (filter != null)
                {
                    query = query.Where(filter);
                }
            }
            return query;
        }

        private async Task<bool> CheckFilterIsExist(string filterId)
        {
            return await _filtersCollection.Find(x => x.Id == filterId).Limit(1).AnyAsync();
        }
    }
}