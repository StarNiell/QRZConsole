using QRZConsole.Core.Settings;

namespace QRZConsole.Tests
{
    public class AppSettingsTests
    {
        [Fact]
        public void Uses_the_registry_key_names_of_older_versions()
        {
            var store = new InMemorySettingsStore();
            var settings = new AppSettings(store)
            {
                Username = "IU8NQI",
                LastLookup = "K1ABC",
                LastFrequency = "7.076",
                ViewMode = "text",
                IsLoggedIn = true,
                CheckIsLoggedAtStartup = true,
                ShowHeader = false
            };

            Assert.Equal("IU8NQI", store.Get("username"));
            Assert.Equal("K1ABC", store.Get("lookup"));
            Assert.Equal("7.076", store.Get("lastFreq"));
            Assert.Equal("text", store.Get("currentViewMode"));
            Assert.Equal("1", store.Get("IsLoggedIn"));
            Assert.Equal("1", store.Get("CheckIsLoggedIn"));
            Assert.Equal("0", store.Get("ShowHeader"));
            Assert.True(settings.IsLoggedIn);
        }

        [Fact]
        public void Reads_locators_saved_with_Newtonsoft_by_older_versions()
        {
            var store = new InMemorySettingsStore();
            store.Set("IU8NQI_Locators", "[{\"Key\":\"JN70\",\"Value\":true},{\"Key\":\"JN61\",\"Value\":false}]");

            List<KeyValuePair<string, bool>> locators = new AppSettings(store).GetLocators("IU8NQI");

            Assert.Equal(2, locators.Count);
            Assert.Equal(new KeyValuePair<string, bool>("JN70", true), locators[0]);
        }

        [Fact]
        public void Missing_or_corrupt_locators_give_an_empty_list()
        {
            var store = new InMemorySettingsStore();
            store.Set("BAD_Locators", "{not json");

            Assert.Empty(new AppSettings(store).GetLocators("NOBODY"));
            Assert.Empty(new AppSettings(store).GetLocators("BAD"));
        }
    }
}
