using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Application.Contracts.Context
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITokenPermissionEncoder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="compressedData"></param>
        /// <returns></returns>
        public string DecompressPermissionData(string compressedData);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public Task<Dictionary<string, object>> EncodeUserPermissionsAsync(long userId, List<string> roles);
    }
}
