using Management.Models;
using System.Runtime.CompilerServices;

namespace Management.Extensions
{
    public static class ResultXNExtensions
    {
        private static readonly ConditionalWeakTable<ResultXN, ResultXNExtraData> _extraData = new();

        public static string GetPreviousResult(this ResultXN resultXN)
        {
            return _extraData.GetOrCreateValue(resultXN).PreviousResult ?? string.Empty;
        }

        public static void SetPreviousResult(this ResultXN resultXN, string value)
        {
            _extraData.GetOrCreateValue(resultXN).PreviousResult = value;
        }

        public static string GetPreviousReturnResultTime(this ResultXN resultXN)
        {
            return _extraData.GetOrCreateValue(resultXN).PreviousReturnResultTime ?? string.Empty;
        }

        public static void SetPreviousReturnResultTime(this ResultXN resultXN, string value)
        {
            _extraData.GetOrCreateValue(resultXN).PreviousReturnResultTime = value;
        }

        public static int GetPreviousStatus(this ResultXN resultXN)
        {
            return _extraData.GetOrCreateValue(resultXN).PreviousStatus;
        }

        public static void SetPreviousStatus(this ResultXN resultXN, int status)
        {
            _extraData.GetOrCreateValue(resultXN).PreviousStatus = status;
        }

        private class ResultXNExtraData
        {
            public string PreviousResult { get; set; } = string.Empty;
            public string PreviousReturnResultTime { get; set; } = string.Empty;
            public int PreviousStatus { get; set; } = 0; // 0 => Bình thường, 1 => Thấp (màu xanh), 2 => Cao (màu đỏ)
        }
    }
}