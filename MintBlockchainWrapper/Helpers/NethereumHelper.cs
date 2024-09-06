using Nethereum.Signer;
namespace MintBlockchainWrapper.Helpers;

internal static class NethereumHelper
{
    public static string GetPublicKey(string privateKey) => new EthECKey(privateKey).GetPublicAddress();
    public static string GetSignature(string messageToSign, string privateKey) => new EthereumMessageSigner().EncodeUTF8AndSign(messageToSign, new EthECKey(privateKey));
}
