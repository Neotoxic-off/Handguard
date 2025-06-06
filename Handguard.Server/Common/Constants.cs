namespace Handguard.Server.Common
{
    public static class Messages
    {
        // Upload user messages
        public const string Upload_NoFile = "No file uploaded.";
        public const string Upload_InvalidFile = "Invalid file.";

        // Upload logs
        public const string Log_UploadNoFile = "Upload request rejected: no file uploaded.";
        public const string Log_UploadInvalidFile = "Upload request rejected: invalid file '{FileName}'.";
        public const string Log_UploadSuccess = "File uploaded successfully. Id: {FileId}";

        // Download messages and logs (from previous steps)
        public const string Download_InvalidRequest = "Id and password are required.";
        public const string Download_NotFound = "File not found or invalid password.";
        public const string Log_DownloadInvalidRequest = "Download request rejected. Invalid parameters. Id: {Id}";
        public const string Log_DownloadFailed = "Download failed. File not found or password mismatch. Id: {Id}";
        public const string Log_DownloadSuccess = "File download successful. Id: {Id}";
    }
}
