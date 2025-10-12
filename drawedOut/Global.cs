namespace drawedOut
{
    ///<summary>
    ///globally used static functions or values
    ///</summary>
    public static class Global
    {
        /// <summary>
        /// Returns the current project's working directory that contains .csproj
        /// </summary>
        /// <returns> String: path to project's directory </returns>
        public static string GetProjFolder()
        {
            DirectoryInfo? dir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent;

            if (dir is null) throw new DirectoryNotFoundException("csproj directory not found");

            return dir.FullName;
        }
    }
}

