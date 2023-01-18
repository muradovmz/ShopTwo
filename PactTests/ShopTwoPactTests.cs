using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PactNet;
using PactNet.Matchers;
using ShopTwo;
using Xunit.Abstractions;

namespace PactTests
{
    public class ShopTwoPactTests
    {
        private readonly IPactBuilderV3 pact;
        private readonly int port = 9000;

        public ShopTwoPactTests(ITestOutputHelper output)
        {
            var config = new PactConfig
            {
                PactDir = "../../../pacts/",
                LogLevel = PactLogLevel.Debug,
                Outputters = (new[] { new XUnitOutput(output) }),
                DefaultJsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };

            String provider = Environment.GetEnvironmentVariable("PACT_PROVIDER");
            // you select which specification version you wish to use by calling either V2 or V3
            IPactV3 pact = Pact.V3("ShopTwo", provider != null ? provider : "Product.API", config);

            // the pact builder is created in the constructor so it's unique to each test
            this.pact = pact.UsingNativeBackend(port);
        }

        [Fact]
        public async void GetProducts_WhenCalled_ReturnsAllProducts()
        {
            //Arrange
            pact
                .UponReceiving("a request to retrieve all products")
                .WithRequest(HttpMethod.Get, "/Product/list")
                .WillRespond()
                .WithStatus(System.Net.HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(Match.MinType(new
                {
                    id = 1,
                    name = "ragaca1",
                    price = 10,
                    weight = 11
                }, 1));

            //Act
            await pact.VerifyAsync(async ctx =>
            {
                var client = new ProductClient();
                var products = await client.GetProducts(ctx.MockServerUri.AbsoluteUri, null);

                //Assert
                Assert.IsType<int>(products[0].Id);
                Assert.IsType<string>(products[0].Name);
                Assert.IsType<double>(products[0].Price);
                Assert.IsType<double>(products[0].Weight);
            });
            //the mock server is no longer running once VerifyAsync returns
        }

        [Fact]
        public async void GetProduct_WhenCalledWithExistingId_ReturnsProduct()
        {
            pact
                .UponReceiving("a request to retrieve a product with existing id")
                .WithRequest(HttpMethod.Get, "/Product/2")
                .WillRespond()
                .WithStatus(System.Net.HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = Match.Type(2),
                    name = Match.Type("ragaca2"),
                    price = Match.Type(20),
                    weight = Match.Type(11)
                });

            //Act
            await pact.VerifyAsync(async ctx =>
            {
                var client = new ProductClient();
                var product = await client.GetProduct(ctx.MockServerUri.AbsoluteUri, 2, null);

                //Assert
                Assert.IsType<int>(product.Id);
                Assert.IsType<string>(product.Name);
                Assert.IsType<double>(product.Price);
            });
        }

        //[Fact]
        //public async void GetProduct_WhenCalledWithInvalidID_ReturnsError()
        //{
        //    pact
        //        .UponReceiving("a request to retrieve a product id that does not exist")
        //        .WithRequest(HttpMethod.Get, "/Products/10")
        //        .WillRespond()
        //        .WithStatus(System.Net.HttpStatusCode.NotFound)
        //        .WithHeader("Content-Type", "application/json; charset=utf-8");


        //    //Act
        //    await pact.VerifyAsync(async ctx =>
        //    {
        //        var client = new ProductClient();

        //        //Assert
        //        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetProduct(ctx.MockServerUri.AbsoluteUri, 10, null));
        //        Assert.Equal("Response status code does not indicate success: 404 (Not Found).", ex.Message);
        //    });
        //}
    }
}
