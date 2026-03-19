using Newtonsoft.Json;
using System;
using System.IO;

public static class LifeTimeLogManager
{
    private static readonly object _lock = new object();

    /// <summary>
    /// Ghi log mới – xóa log cũ – chỉ giữ 1 log
    /// </summary>
    public static void AddOrReplaceLog(LifeTimeLog ltlog, string logFilePath)
    {
        if (ltlog == null)
            throw new ArgumentNullException(nameof(ltlog));

        lock (_lock)
        {
            string json = JsonConvert.SerializeObject(
                ltlog,
                Formatting.Indented);

            string dir = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Ghi file atomic: ghi temp trước
            string tempFile = logFilePath + ".tmp";

            File.WriteAllText(tempFile, json);

            // Replace an toàn
            if (File.Exists(logFilePath))
                File.Replace(tempFile, logFilePath, null);
            else
                File.Move(tempFile, logFilePath);
        }
    }


    /// <summary>
    /// Đọc log hiện tại (chỉ 1)
    /// </summary>
    public static LifeTimeLog LoadLog(string LogFilePath)
    {
        if (!File.Exists(LogFilePath))
            return null;

        try
        {
            string json = File.ReadAllText(LogFilePath);
            return JsonConvert.DeserializeObject<LifeTimeLog>(json);
        }
        catch
        {
            // File lỗi → coi như không có log
            return null;
        }
    }

    /// <summary>
    /// Xóa log
    /// </summary>
    public static void ClearLog(string LogFilePath)
    {
        lock (_lock)
        {
            if (File.Exists(LogFilePath))
                File.Delete(LogFilePath);
        }
    }
}
