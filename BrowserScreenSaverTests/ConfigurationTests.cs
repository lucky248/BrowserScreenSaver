using System;
using BrowserScreenSaver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrowserScreenSaverTests
{
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void ConfigurationDefaultTest()
        {
            var config = AppConfiguration.FromString(null);
            Assert.AreEqual(8, config.Panes.Count);

            config = AppConfiguration.FromString(string.Empty);
            var defaultConfig = config.ToString();
            var expectedConfig = @"v1
0.5
0.5
0.5
True
1/1/2000 12:00:00 AM
0

-1
100
100

-1
100
100

-1
100
100

-1
100
100

-1
100
100

-1
100
100

-1
100
100

-1
100
100
";
            Assert.AreEqual(expectedConfig, defaultConfig);
        }

        [TestMethod]
        public void ConfigurationTest()
        {
            AppConfiguration appConfig = new AppConfiguration();
            appConfig.SharedConfig.BottomVerticalSplitter = 0.1;
            appConfig.SharedConfig.HorizontalSplitter = 0.2;
            appConfig.SharedConfig.NavigationEnabledByUtc = new DateTime(year: 2001, month: 2, day: 3);
            appConfig.SharedConfig.OnResumeDisplayLogon = true;
            appConfig.SharedConfig.SafeUris.Add(new Uri("http://one.two/3?4"));
            appConfig.SharedConfig.SafeUris.Add(new Uri("http://fice.six/7?8"));
            appConfig.SharedConfig.TopVerticalSplitter = 0.3;

            for (int i=0; i < appConfig.Panes.Count; i++)
            {
                appConfig.Panes[i].RefreshFreq = i * 10;
                appConfig.Panes[i].Scale = i * 11;
                appConfig.Panes[i].ScaleMaximized = i * 12;
                appConfig.Panes[i].Uri = i %3 == 0 ? null : "https://nine.com/10?" + i;
            }

            var one = appConfig.ToString();
            var two = AppConfiguration.FromString(one).ToString();
            Assert.AreEqual(one, two);

            var rawValue = @"v1
0.3
0.1
0.2
True
2/3/2001 12:00:00 AM
2
aAB0AHQAcAA6AC8ALwBvAG4AZQAuAHQAdwBvAC8AMwA/ADQA
aAB0AHQAcAA6AC8ALwBmAGkAYwBlAC4AcwBpAHgALwA3AD8AOAA=

0
0
0
aAB0AHQAcABzADoALwAvAG4AaQBuAGUALgBjAG8AbQAvADEAMAA/ADEA
10
11
12
aAB0AHQAcABzADoALwAvAG4AaQBuAGUALgBjAG8AbQAvADEAMAA/ADIA
20
22
24

30
33
36
aAB0AHQAcABzADoALwAvAG4AaQBuAGUALgBjAG8AbQAvADEAMAA/ADQA
40
44
48
aAB0AHQAcABzADoALwAvAG4AaQBuAGUALgBjAG8AbQAvADEAMAA/ADUA
50
55
60

60
66
72
aAB0AHQAcABzADoALwAvAG4AaQBuAGUALgBjAG8AbQAvADEAMAA/ADcA
70
77
84
";

            Assert.AreEqual(rawValue, one);
        }
    }
}
