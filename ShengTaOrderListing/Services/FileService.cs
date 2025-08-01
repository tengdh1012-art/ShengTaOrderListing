using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace ShengTaOrderListing.Services
{
    public class FileService
    {
        private readonly IJSRuntime _jsRuntime;

        public FileService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SaveAsFile(string filename, byte[] data, string contentType)
        {
            await _jsRuntime.InvokeVoidAsync(
                "saveAsFile",
                filename,
                contentType,
                Convert.ToBase64String(data));
        }
    }
}