using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bts.udpgateway
{
    public class RegisterSMSData : DapperBaseData<RegisterSMS>
    {

        public RegisterSMSData()
        {

        }
        public IEnumerable<RegisterSMS> GetRegisterSMS(int deviceid)
        {
            string query = string.Format(@"select *from RegisterSMS where DeviceId={0}", deviceid);
            return Query<RegisterSMS>(query, null);
        }

    }
}
