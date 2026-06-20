using System.Net.NetworkInformation;

namespace Management.BL
{
    public class MyHostedService: IHostedService
    {
        public readonly APIBL _apiBL;
        public readonly APIBL_MHIS _apiBL_MHIS;
        public MyHostedService(APIBL apiBL, APIBL_MHIS aPIBL_MHIS)
        {
            _apiBL = apiBL;
            _apiBL_MHIS = aPIBL_MHIS;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var _settingAPIToStart = await _apiBL.GetSetting_APIStartAutoTask();
            if (!string.IsNullOrEmpty(_settingAPIToStart))
            {
                if (_settingAPIToStart == "1") // Đức Tâm
                {
                    await _apiBL.StartAutoTask();
                }
                else if (_settingAPIToStart == "2") // MHIS
                {
                    await _apiBL_MHIS.StartAutoTask();
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //Cleanup logic here
        }
    }
}
