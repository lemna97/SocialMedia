using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.OpenAI
{
    public interface IChatGPTService
    { 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public Task<string> GetChatGPTResponseAsync(List<dynamic> messages); 
    }
}
