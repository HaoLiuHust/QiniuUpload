using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qiniu.Util;
using Qiniu.Storage;
using Qiniu.Http;
using Newtonsoft.Json;

namespace QiniuUpload
{
    class RemoteFileInfo
    {
        public static string RemoteFileStat(Mac iMac, string iBucket, string iFileName)
        {
            BucketManager lBktMgr = new BucketManager(iMac);

           // Qiniu.Storage.Model.StatResult lRslt = null;
            //StringBuilder lOpsb = new StringBuilder();
            HttpResult result = null;
            result = lBktMgr.batch("op=/stat/" + StringUtils.encodedEntry(iBucket, iFileName));

           // lRslt = lBktMgr.stat(iBucket, iFileName);
            if(result == null)
            {
                return string.Empty;
            }
            StatResponse lStatResponse = null;
            lStatResponse = JsonConvert.DeserializeObject<StatResponse>(result.Response);
            if (lStatResponse.CODE == 200)
                return lStatResponse.DATA.hash;
            else
                return String.Empty;
        }
    }

    /// <summary>
    /// Batch请求返回的JSON格式字符串(数组)
    /// 以下是一个示例
    /// 
    /// [
    ///   {
    ///         "code":200,
    ///         "data":
    ///             {
    ///                 "fsize":16380,
    ///                 "hash":"FjBkn9ObUVW1Z9GvmKbbAUEp3gwE",
    ///                 "mimeType":"image/jpeg",
    ///                 "putTime":14742756456724365
    ///             }
    ///   },
    ///   {
    ///         "code":612,
    ///         "data":
    ///             {
    ///                 "error":"no such file or directory"
    ///             }
    ///   }
    /// ]
    /// </summary>
    internal class StatResponse
    {
        public int CODE { get; set; }
        public Meta DATA { get; set; }
    }

    /// <summary>
    /// Stat的Data部分
    ///  {
    ///     "fsize":16380,
    ///     "hash":"FjBkn9ObUVW1Z9GvmKbbAUEp3gwE",
    ///     "mimeType":"image/jpeg",
    ///     "putTime":14742756456724365
    ///   }
    /// </summary>
    internal class Meta
    {
        public long fsize { get; set; }
        public string hash { get; set; }

        public string mimeType { get; set; }

        public long putTime { get; set; }
    }
}
