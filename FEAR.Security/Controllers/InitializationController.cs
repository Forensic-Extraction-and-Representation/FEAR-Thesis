using FEAR.Domain.Model.ProtectedEncryptionKey;
using FEAR.Host.Core.ProtectedEncryptionKey;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FEAR.Security.Controllers
{
    [ApiController]
    [Route("/v1.0/System/[controller]/[action]")]
    public partial class InitializationController : Controller
    {
        IConfiguration _configuration;
        IProtectedEncryptionKeyProvider _keyProvider;
        public InitializationController(IProtectedEncryptionKeyProvider keyProvider, IConfiguration configuration)
        {
            _configuration = configuration;
            if (!_configuration.GetSection("Initialization").Get<ProtectedEncryptionKeyBuilderOptions>().Enabled)
                throw new NotImplementedException();
            _keyProvider = keyProvider;
        }

        private byte[] RandomStringGenerator(int length)
        {
            Random _random = new Random();
            // chars needs to be upper, lower, number and basic symbols
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+{}|:<>?[];',./`~";
            var s = new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
            return Encoding.UTF8.GetBytes(s);
        }

        [HttpGet]
        [Route("/v1.0/System/[controller]")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult KeySetup()
        {
            if (_keyProvider.KeySetupEnabled)
                return View();
            else
                return NotFound();
        }
        [HttpPost]
        public IActionResult SubmitKey([FromForm]SubmitKeyModel data)
        {
            bool success = _keyProvider.SubmitKey(data);

            return View();
        }

        [HttpPost]
        public IActionResult EncryptData([FromForm] EncryptDataModel data)
        {
            // Create a dictionary from the keyIdentifiers and keyValues
            // and pass it to the AddEncryptedKey method.
            var keyDict = new Dictionary<string, string>();
            for (int i = 0; i < data.keyIdentifiers.Length; i++)
            {
                keyDict.Add(data.keyIdentifiers[i], data.keyValues[i]);
            }

            byte[] keyData = data.autoGenerateKey ? RandomStringGenerator(32) : Encoding.UTF8.GetBytes(data.keyValueTextarea);
            _keyProvider.AddEncryptedKey(data.keyName, keyDict, keyData);
            return View();
        }
    }
}
