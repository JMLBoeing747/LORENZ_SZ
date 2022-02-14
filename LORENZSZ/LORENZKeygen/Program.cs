using Cryptography;
using System;
using System.IO;

namespace LORENZKeygen
{
    class Program
    {
        private static string UserinfoTextFile { get => "USERINFO.TXT"; }

        public static (string, string) DechiffrerUserInfo()
        {
            Decyphering.OpeningDecyphering(UserinfoTextFile, out uint[] keyQBytes, out uint[] cypheredMessageOnly);
            Keygen.CreateMatrix(ref keyQBytes, -3);
            Common.XORPassIntoMessage(keyQBytes, ref cypheredMessageOnly);
            Common.ReverseKey(ref keyQBytes);
            Common.NotOperationToKey(ref keyQBytes);
            Keygen.CreateMatrix(ref keyQBytes, -2);
            Common.XORPassIntoMessage(keyQBytes, ref cypheredMessageOnly);

            //Strip out unknown characters, associate and verifying infos...
            (string, string, DateTime, string) userInfos = Decyphering.ShortingUserInfos(Decyphering.StripOutAndSplit(cypheredMessageOnly));
            DateTime dtLimit = userInfos.Item3.AddSeconds(30.0);
            if (userInfos.Item3 < DateTime.UtcNow && dtLimit > DateTime.UtcNow)
            {
                //Show caracteristics
                Display.PrintMessage("USERNAME : " + ShowHiddenInfos(userInfos.Item1), MessageState.Info);
                Display.PrintMessage("COMPUTER NAME : " + ShowHiddenInfos(userInfos.Item2), MessageState.Info);
                Console.ReadKey(true);
                File.Delete(UserinfoTextFile);
                return (userInfos.Item1, userInfos.Item2);
            }
            else if (userInfos.Item3 > DateTime.UtcNow)
                throw new KeygenException("Cypher datetime was incoherent! => " + userInfos.Item3 + " UTC.");
            else
                throw new KeygenException("Cypher has expired since " + dtLimit + " UTC.");

        }

        static string ShowHiddenInfos(string info)
        {
            string hiddenInfo = default;
            int showCharNbr = info.Length >= 3 ? (int)Math.Floor(2 * Math.Log(info.Length - 2)) : 0;
            for (int c = 0; c < info.Length; c++)
                if (c >= info.Length - showCharNbr)
                    hiddenInfo += info[c];
                else
                    hiddenInfo += '*';
            return hiddenInfo;
        }

        public static void Main()
        {
            Keygen.GeneratingKey(DechiffrerUserInfo());
        }
    }
}
