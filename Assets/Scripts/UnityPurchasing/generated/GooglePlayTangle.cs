// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("9XZ4d0f1dn119XZ2d9NRjTG5Z4JH9XZVR3pxfl3xP/GAenZ2dnJ3dAdk+YMtiNphMCEU4ecrBwUZHOSj77Qxt/5HeaOEszhN2IRsOgsmYqCSnlI6MKw+fOF8aDzzuZM5PebhfGNHVdek92jzEFJDpB6Bh54phqOeWi+OtpxkPdrYo+1xmA2GN/FoO4R/L9KQNY/qGGfkqRTKnX5+IMVwtW5daxqBmuevFh1dd0K7URbDQ27T76JE9EjofQCPrQETj5muADYsmcQrYNvyWkhXibaSra206j7fPypo4zX72Hy95sTD+7JfRvbvhBThlWVZzophOofjjuav6tbqMVX5lqe3fokZsWhG7w/0RJKRCUoQGTt4WA/sEuuoYglrcwl+8HV0dnd2");
        private static int[] order = new int[] { 1,1,6,12,7,5,8,9,8,11,13,12,12,13,14 };
        private static int key = 119;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
