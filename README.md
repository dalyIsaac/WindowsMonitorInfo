# WindowsMonitorInfo

A proof-of-concept which to test whether a Windows App SDK application can acquire the correct scale through `GetScaleFactorForMonitor` (via CsWin32), and calculate it manually.

When `<WindowsPackageType>None</WindowsPackageType>` is set, the application will run in a desktop environment, and the scale factor will be reported incorrectly.

This anomaly was also encountered in this response on [StackOverflow](<https://stackoverflow.com/questions/33507031/detect-if-non-dpi-aware-application-has-been-scaled-virtualized/36864741#36864741:~:text=On%20a%20Windows%2010%20system%20where%20the%20system%20DPI%20is%2096%2C%20and%20a%20high%2DDPI%20monitor%20has%20a%20144%20DPI%20(150%25%20scaling)%2C%20the%20GetScaleFactorForMonitor%20function%20returns%20SCALE_140_PERCENT%20when%20it%20would%20be%20expected%20to%20return%20SCALE_150_PERCENT%20(144/96%20%3D%3D%201.5).>).

## Correct Reporting

1. On a secondary monitor, set the display scaling to 175%
2. Ensure `<WindowsPackageType>None</WindowsPackageType>` is **not set**
3. Run the application

Then, the reported scale factor will be correctly stated as 175%.

## Incorrect Reporting

1. On a secondary monitor, set the display scaling to 175%
2. Ensure `<WindowsPackageType>None</WindowsPackageType>` is set
3. Run the application

Then, the reported scale factor will be incorrectly stated as 140%.
