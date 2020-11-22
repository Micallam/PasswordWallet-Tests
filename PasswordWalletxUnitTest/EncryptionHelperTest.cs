using Autofac.Extras.Moq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Moq;
using PasswordWallet;
using System;
using System.ComponentModel;
using System.Text;
using Xunit;

namespace PasswordWalletxUnitTest
{
    public class EncryptionHelperTest
    {
        [Theory]
        [InlineData("abcd", "E2-FC-71-4C-47-27-EE-93-95-F3-24-CD-2E-7F-33-1F")]
        [InlineData("TestValue", "88-CD-0D-DD-51-3F-40-D7-88-32-BE-D8-4A-AE-6C-6D")]
        [InlineData("Sp3c!@l", "54-0E-67-D3-6F-C0-B0-83-1D-3C-22-0B-D4-30-6C-DD")]
        public void TestCalculateMD5_ReturnsExpectedResult_IfStringValuePassed(string value, string expected)
        {
            EncryptionHelper encryption = new EncryptionHelper();

            byte[] actualByte = encryption.CalculateMD5(value);
            string actual = BitConverter.ToString(actualByte);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("abcd", "e2fc714c4727ee9395f324cd2e7f331f", "X1bJQrnQfHi49HBD8LOUlw==")]
        [InlineData("Sp3c!@l", "e2fc714c4727ee9395f324cd2e7f331f", "t0cVFyxgtAqha4xGEYhURQ==")]
        public void TestEncryptAES_GeneratesExpectedResult_IfStringValuePassed(string value, string keyString, string expected)
        {
            EncryptionHelper encryption = new EncryptionHelper();
            byte[] key = Encoding.ASCII.GetBytes(keyString);

            string actual = encryption.EncryptAES(value, key);

            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("X1bJQrnQfHi49HBD8LOUlw==", "e2fc714c4727ee9395f324cd2e7f331f", "abcd")]
        [InlineData("t0cVFyxgtAqha4xGEYhURQ==", "e2fc714c4727ee9395f324cd2e7f331f", "Sp3c!@l")]
        public void TestDecryptAES_DecryptsGivenValues_IfEncryptedValueIsPassed(string value, string keyString, string expected)
        {
            EncryptionHelper encryption = new EncryptionHelper();
            byte[] key = Encoding.ASCII.GetBytes(keyString);

            string actual = encryption.DecryptAES(value, key);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestEncryptAES_ThrowsException_WhenKeyIsNotPassed()
        {
            EncryptionHelper encryption = new EncryptionHelper();

            string value = "value";
            byte[] key = null;

            Assert.Throws<ArgumentNullException>(
               () => encryption.EncryptAES(value, key)
               );
        }

        [Fact]
        public void TestDecryptAES_ThrowsException_WhenKeyIsNotPassed()
        {
            EncryptionHelper encryption = new EncryptionHelper();

            string value = "value";
            byte[] key = null;

            Assert.Throws<ArgumentNullException>(
               () => encryption.DecryptAES(value, key)
               );
        }
    }
}
