namespace GUI.Helpers;

public static class FileHelper
{
	public static async Task OpenFileAsync(string filePath)
	{

		if (!File.Exists(filePath)) throw new ArgumentNullException(nameof(filePath));

		var fileRequest = new OpenFileRequest
		{
			File = new ReadOnlyFile(filePath)
		};

		await Launcher.Default.OpenAsync(fileRequest);
	}
}
