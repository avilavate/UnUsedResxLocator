# UnUsedResxLocator
A small utility to find all the used resource keys (usually used for localization in asp.net projects). tool also does a static code analysis to find if a key is being used in views/controllers which has not initialized in resources.resx file.
# How to Run
Make changes in app.config as per your requirments, it needs a dir path where it can find .resx file used for you MVC app and path to the directory (where you want to scan resx key references)
