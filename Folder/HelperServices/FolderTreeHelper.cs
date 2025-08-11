using Folder.Entities;

namespace Folder.HelperServices
{
    public static class FolderTreeHelper
    {
        public static FolderEntity<TFile>? FindFolderRecursive<TFile>(FolderEntity<TFile> currentFolder, string path) where TFile : BaseFile
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

        public static void UpdateParentDates<TFile>(FolderEntity<TFile> rootFolder, string path) where TFile : BaseFile
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                return;

            for (int i = segments.Length; i > 0; i--)
            {
                var partialPath = "/" + string.Join("/", segments.Take(i)); // /Users/A/B, sonra /Users/A, sonra /Users
                var folder = FolderTreeHelper.FindFolderRecursive(rootFolder, partialPath);
                if (folder != null)
                    folder.UpdateDate = DateTime.UtcNow;
            }

            rootFolder.UpdateDate = DateTime.UtcNow;
        }

        public static FolderEntity<TFile> DeepCloneWithNewPath<TFile>(FolderEntity<TFile> source, string oldBasePath, string newBasePath) where TFile : BaseFile
        {
            var clone = new FolderEntity<TFile>
            {
                Name = source.Name,
                Path = source.Path.Replace(oldBasePath, newBasePath),
                Icon = source.Icon,
                Comment = source.Comment,
                Files = source.Files.ToList(),
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                Children = new List<FolderEntity<TFile>>()
            };

            foreach (var child in source.Children)
            {
                clone.Children.Add(DeepCloneWithNewPath(child, oldBasePath, newBasePath));
            }

            return clone;
        }

        public static List<string> GetAllCodesWithPrefix<TFile>(FolderEntity<TFile> folder, string prefix) where TFile : BaseFile
        {
            var result = new List<string>();

            void collect(FolderEntity<TFile> current)
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