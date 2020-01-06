using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nenter.Blog.Application.ModelConvention
{
    /*
     services.AddMvc(options =>
         {
             options.Conventions.Add(new ApiControllerAuthorizeConvention());
         })


再改进一下
实际开发中我的AccessControlFilter需要通过构造函数注入业务接口，类似于这样：

    public class AccessControlFilter : IActionFilter
    {
        private IUserService _userService;

        public AccessControlFilter(IUserService service)
        {
            _userService = service;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
                //模拟一下业务操作
                //var user=_userService.GetById(996);
                //.......
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
如何优雅的在Convention中使用DI自动注入呢？Asp.Net Core MVC框架提供的ServiceFilter可以解决这个问题，ServiceFilter本身是一个过滤器，它的不同之处在于能够通过构造函数接收一个Type类型的参数，我们可以在这里把真正要用的过滤器传进去，于是上面的过滤器注册过程演变为：

    controller.Filters.Add(new ServiceFilterAttribute(typeof(AccessControlFilter)));
当然了，要从DI中获取这个filter实例，必须要把它注入到DI容器中：

    services.AddScoped<AccessControlFilter>();

   */
    public class ApiControllerAuthorizeConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (controller.Filters.Any(x => x is ApiControllerAttribute) && !controller.Filters.Any(x => x is AccessControlFilter))
                {
                    controller.Filters.Add(new AccessControlFilter());
                }
            }
        }
    }
}
