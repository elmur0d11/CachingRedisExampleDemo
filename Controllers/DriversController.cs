using CachingRedis.Data;
using CachingRedis.Models;
using CachingRedis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CachingRedis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        #region DIContainer
        private readonly ILogger<DriversController> _logger;
        private readonly ICacheService _cacheService;
        private readonly AppDbContext _context;


        public DriversController(ILogger<DriversController> logger, ICacheService cacheService, AppDbContext context)
        {
            _logger = logger;
            _cacheService = cacheService;
            _context = context;
        }
        #endregion

        #region Get
        [HttpGet("drivers")]
        public async Task<IActionResult> Get()
        {
            //Get data from cache with key drivers and put data to variable cacheData
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");

            /*
             Check variable cacheData
            if cacheData ain't null and Count ain't equal to 0
            we will return cacheData to server
            if cacheData is equal to null or count is equal to 0.
            We will get all data from our Database and we will put data to 
            cacheData. After that we will create expiration Time for 30 second.
            And we will use method SetData was created on ICacheService
            after that all we will return all the data to server.
             */
            if (cacheData != null && cacheData.Count() > 0)
                return Ok(cacheData);

            //Get data from Database
            cacheData = await _context.Drivers.ToListAsync();

            //Create Expiration Time and put this to variable expiryTime
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);

            //Calling to method SetData
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiryTime);

            //Return data to server
            return Ok(cacheData);
        }
        #endregion

        #region Post
        [HttpPost("AddDriver")]
        public async Task<IActionResult> Post(Driver value)
        {
            /*
            1.AddAsync(): Build-In method for add data to database
            we will add value to database and we will put that into
            variable addedObj.
            2.We will cretae expiration time
            after that we will use our method SetData for add new value to cache
            we will give key, addedObj and expiryTime
            3.After That we will use Build-In method SaveChangesAsync()
            for add all data to real database
            4.And we will return new addedObj to Server
             */
            var addedObj = await _context.Drivers.AddAsync(value);

            //Create Expiration Time
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);

            //Add new Data to Cache
            _cacheService.SetData<Driver>($"driver{value.Id}", addedObj.Entity, expiryTime);

            //Save Changes (add new data to real DB)
            await _context.SaveChangesAsync();

            //return new data to user
            return Ok(addedObj.Entity);
        }
        #endregion

        #region Delete
        [HttpDelete("DeleteDriver")]
        public async Task<IActionResult> Delete(int id)
        {
            /*
            1.We will get data from db using id,
            FirstOrDefaultAsync(): Helps for find object from id.
            we will find object if object founded succesfuly method will return
            founded data else method return null.
            After that we will put founded data into variable exist
            2.After that we will check variable exist.
            If exist ain't equal to null we will =>
            use Build-In Method Remove() for delete data from database.
            and we will use our RemoveData() method for delete data from cache
            After that we will use Build-In method SaveChanges() for delete data from real DB
            And we will return NoContent() Action Result to server.
            Else variable exist equal to null.
            we will return NotFound result to server
            */
            //Get Data from db using id
            var exist = _context.Drivers.FirstOrDefaultAsync(x => x.Id == id);

            //checking variable exist
            if (exist != null)
            {
                //Remove Data from Database
                _context.Remove(exist);

                //Remove Data from cache
                _cacheService.RemoveData($"driver{id}");

                //Save Changes (Delete data from real db)
                await _context.SaveChangesAsync();

                //return nocontent result to server
                return NoContent();
            }

            //Not Found action result 
            return NotFound("Object notfound. error: This is Uzbekistan");
        }
        #endregion
    }
}
