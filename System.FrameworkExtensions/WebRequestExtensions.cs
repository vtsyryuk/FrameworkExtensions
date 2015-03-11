using System.IO;
using System.Threading.Tasks;

namespace System.Net
{
	public static class WebRequestExtensions
	{
		public static Task<Stream> GetRequestStreamAsync(this HttpWebRequest req)
		{
			return Task.Factory.FromAsync<Stream>(req.BeginGetRequestStream, req.EndGetRequestStream, null);
		}

		public static Task<WebResponse> GetResponseAsync(this HttpWebRequest req)
		{
			return Task.Factory.FromAsync<WebResponse>(req.BeginGetResponse, req.EndGetResponse, null);
		}
	}
}