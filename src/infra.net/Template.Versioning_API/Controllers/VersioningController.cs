using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.NoVersion
{
    [ApiController]
    [Route("[controller]")]
    public class NoVersionController : ControllerBase
    {
        [HttpGet]
        public string Get() => "v1.0";

    }
    [ApiController]
    [ApiVersion("2.0")]
    [Route("[controller]")]
    public class OnlyV2Controller : ControllerBase
    {
        [HttpGet]
        public string Get() => "v2.0";
    }
}
namespace WebApi.Controllers.v1_0
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [Route("v{version:apiVersion}/Versioning")]
    public class VersioningController : ControllerBase
    {

        [HttpGet]
        public string Get() => "v1.0";

    }
}

namespace WebApi.Controllers.v2_0
{

    [ApiController]
    [ApiVersion("2.0")]
    [Route("[controller]")]
    [Route("v{version:apiVersion}/Versioning")]
    public class VersioningController : ControllerBase
    {
        [HttpGet, MapToApiVersion("2.0")]
        public string Get() => "v2.0";

    }
}
