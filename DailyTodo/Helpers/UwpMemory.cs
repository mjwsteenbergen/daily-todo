using ApiLibs.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace DailyTodo.Helpers
{
    class UwpMemory : AsyncMemory
    {
        public async override Task<string> Read(string filename)
        {
            StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(filename);
            return await FileIO.ReadTextAsync(file);
        }

        public async override Task WriteString(string filename, string obj)
        {
            try
            {
                await ApplicationData.Current.RoamingFolder.CreateFileAsync(filename, CreationCollisionOption.FailIfExists);
            }
            catch { }
            StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(filename);
            await FileIO.WriteTextAsync(file, obj);
        }
    }
}
