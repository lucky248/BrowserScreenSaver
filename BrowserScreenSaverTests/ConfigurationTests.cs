﻿using System;
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
            Assert.AreEqual(AppConfiguration.SupportedWindowCount, config.Windows.Count);
            Assert.AreEqual(4, config.Windows[0].Panes.Count);
            Assert.AreEqual(4, config.Windows[1].Panes.Count);

            config = AppConfiguration.FromString(string.Empty);
            var defaultConfig = config.ToString();
            var expectedConfig = @"2
True
0
1/1/2000 12:00:00 AM
0
3
0.5
0.5
0.5
4

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
0.5
0.5
0.5
4

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
0.5
0.5
0.5
4

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
        public void ConfigurationRoundtripTest()
        {
            AppConfiguration appConfig = new AppConfiguration();
            appConfig.SharedWindowConfig.OnResumeDisplayLogon = false;
            appConfig.SharedWindowConfig.StartupDelaySec = 123;
            appConfig.SharedPanelConfig.SafeUris.Add(new Uri("http://one.two/3?4"));
            appConfig.SharedPanelConfig.SafeUris.Add(new Uri("http://fice.six/7?8"));

            for (int w = 0; w < appConfig.Windows.Count; w++)
            {
                appConfig.Windows[w].BottomVerticalSplitter = 0.1 * w;
                appConfig.Windows[w].HorizontalSplitter = 0.2 * w;
                appConfig.Windows[w].TopVerticalSplitter = 0.3 * w;
                appConfig.SharedPanelConfig.NavigationEnabledByUtc = new DateTime(year: 2001, month: 2, day: 3);

                for (int p = 0; p < appConfig.Windows[w].Panes.Count; p++)
                {
                    appConfig.Windows[w].Panes[p].RefreshFreq = p * 10 + w;
                    appConfig.Windows[w].Panes[p].Scale = p * 11 + w;
                    appConfig.Windows[w].Panes[p].ScaleMaximized = p * 12 + w;
                    appConfig.Windows[w].Panes[p].Uri = p % 3 == 0 ? null : "https://nine.com/10?" + p;
                }
            }

            var serialized1 = appConfig.ToString();
            var appConfig2 = AppConfiguration.FromString(serialized1);
            var serialized2 = appConfig2.ToString();
            Assert.AreEqual(serialized1, serialized2);

            var rawValue = @"2
False
123
2/3/2001 12:00:00 AM
2
aAB0AHQAcAA6AC8ALwBvAG4AZQAuAHQAdwBvAC8AMwA/ADQA
aAB0AHQAcAA6AC8ALwBmAGkAYwBlAC4AcwBpAHgALwA3AD8AOAA=
3
0
0
0
4

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
0.3
0.1
0.2
4

1
1
1
aAB0AHQAcABzADoALwAvAG4AaQBuAGUALgBjAG8AbQAvADEAMAA/ADEA
11
12
13
aAB0AHQAcABzADoALwAvAG4AaQBuAGUALgBjAG8AbQAvADEAMAA/ADIA
21
23
25

31
34
37
0.6
0.2
0.4
4

2
2
2
aAB0AHQAcABzADoALwAvAG4AaQBuAGUALgBjAG8AbQAvADEAMAA/ADEA
12
13
14
aAB0AHQAcABzADoALwAvAG4AaQBuAGUALgBjAG8AbQAvADEAMAA/ADIA
22
24
26

32
35
38
";

            Assert.AreEqual(rawValue, serialized1);
        }
    }
}
