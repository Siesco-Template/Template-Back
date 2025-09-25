using Folder.Entities;
using MongoDB.Driver;

namespace Folder.Abstractions
{
    public interface IFolderMongoContext
    {
        IMongoCollection<FolderEntity> GetCollection<TFile>(string collectionName);
    }
}