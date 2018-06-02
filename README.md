#**.NET C# Http Web Proxy**

This project inspired from [PHP Proxy App.](https://github.com/Athlon1600/php-proxy-app)


#**Basic Usage**
```
Request request = new Request(HttpMethod.Get, "http://www.google.com.tr", new NameValueCollection(), new NameValueCollection());
Core.Proxy proxy = new Core.Proxy();
var response = proxy.Forward(request);

Assert.IsNotNull(response);
Assert.IsTrue(!string.IsNullOrEmpty(response.Content));
```

#**Example Project**

You can see and learn how to make basic web proxy page.
