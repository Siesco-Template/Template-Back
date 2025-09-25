using Folder.Entities;

namespace Folder.HelperServices
{
    public static class FolderTreeHelper
    {
        public static FolderEntity? FindFolderRecursive(FolderEntity currentFolder, string path)
        {
            if (currentFolder.Path == path)
                return currentFolder;

            foreach (var child in currentFolder.Children)
            {
                var result = FindFolderRecursive(child, path);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static void UpdateParentDates(FolderEntity rootFolder, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                return;

            for (int i = segments.Length; i > 0; i--)
            {
                var partialPath = "/" + string.Join("/", segments.Take(i)); // /Users/A/B, sonra /Users/A, sonra /Users
                var folder = FindFolderRecursive(rootFolder, partialPath);
                if (folder != null)
                    folder.UpdateDate = DateTime.UtcNow;
            }

            rootFolder.UpdateDate = DateTime.UtcNow;
        }

        public static FolderEntity DeepCloneWithNewPath(FolderEntity source, string oldBasePath, string newBasePath)
        {
            var clone = new FolderEntity
            {
                Name = source.Name,
                Path = source.Path.Replace(oldBasePath, newBasePath),
                Icon = source.Icon,
                Comment = source.Comment,
                Files = source.Files.ToList(),
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                Children = []
            };

            foreach (var child in source.Children)
            {
                clone.Children.Add(DeepCloneWithNewPath(child, oldBasePath, newBasePath));
            }

            return clone;
        }

        public static List<string> GetAllCodesWithPrefix(FolderEntity folder, string prefix)
        {
            var result = new List<string>();

            void collect(FolderEntity current)
            {
                if (current.Files != null)
                {
                    result.AddRange(current.Files
                        .OfType<BaseFile>()
                        .Where(f => f.Code.StartsWith(prefix))
                        .Select(f => f.Code));
                }

                foreach (var child in current.Children)
                {
                    collect(child);
                }
            }

            collect(folder);
            return result;
        }
    }
}