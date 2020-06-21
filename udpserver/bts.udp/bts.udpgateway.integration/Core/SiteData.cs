using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bts.udpgateway
{
    public class SiteData : DapperBaseData<Site>
    {

        public SiteData()
        {

        }
        public IEnumerable<Site> GetSite(int deviceid)
        {
            string query = string.Format(@"select Name from Site where DeviceId={0}", deviceid);
            return Query<Site>(query, null);
        }

    }
}
