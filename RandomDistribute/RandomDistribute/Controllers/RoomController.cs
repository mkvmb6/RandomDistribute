using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Firebase.Database;
using Firebase.Database.Query;

using Microsoft.AspNetCore.Mvc;

using RandomDistribute.Models;

namespace RandomDistribute.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly FirebaseClient myFirebaseClient;
        private readonly IRangeService myRangeService;
        private readonly IRateLimitService myRateLimitService;
        private const string Rooms = "test_rooms";

        private const string MyAlphanumeric =
            "0123456789" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "abcdefghijklmnopqrstuvwxyz";

        public RoomController(FirebaseClient firebaseClient, IRangeService rangeService, IRateLimitService rateLimitService)
        {
            myFirebaseClient = firebaseClient;
            myRangeService = rangeService;
            myRateLimitService = rateLimitService;
        }

        [HttpGet]
        [Route("{shortUrl}")]
        public async Task<IActionResult> GetUrl(string shortUrl)
        {
            var clientIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
            if (clientIpAddress != null && myRateLimitService.IsLimitExceeded(clientIpAddress.ToString(), 5))
            {
                return BadRequest($"Limit Exceeded for your IP: {clientIpAddress}");
            }

            var roomData = await myFirebaseClient.Child(Rooms)
                .Child(shortUrl).OnceAsync<RoomDto>();
            if (roomData?.Any() == false)
            {
                return NotFound("Specified room is not available.");
            }

            return Ok(roomData.Single().Object);
        }

        [HttpPost]
        [Route("addRoom")]
        public async Task<IActionResult> AddRoom([FromBody] string roomOwner)
        {
            var clientIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
            if (clientIpAddress != null && myRateLimitService.IsLimitExceeded(clientIpAddress.ToString()))
            {
                return BadRequest($"Limit Exceeded for your IP: {clientIpAddress}");
            }

            if (!IsRequestValid(roomOwner, out var badRequest))
            {
                return badRequest;
            }

            long num;
            lock (myRangeService.NumberRange)
            {
                num = myRangeService.NumberRange.Current;
                ++myRangeService.NumberRange.Current;
                if (IsNextRangeDue())
                {
                    myRangeService.UpdateNextNumberRange();
                }
                else if (myRangeService.NumberRange.Current > myRangeService.NumberRange.End)
                {
                    myRangeService.NumberRange = myRangeService.NextNumberRange;
                }
            }

            var shortUrl = GetBase62Url(num);
            //await myFirebaseClient.Child(Rooms).Child(shortUrl).PutAsync(new RoomDto
            //{
            //    Users = new Dictionary<string, string> { { roomUser, string.Empty } }
            //});
            await myFirebaseClient.Child(Rooms).Child(shortUrl).PostAsync<RoomDto>(new RoomDto
            {
                RoomOwner = roomOwner,
                Users = new Dictionary<string, string> {{roomOwner, string.Empty}}
            });
            return Ok(new { ShortUrl = shortUrl });
        }

        [HttpPut]
        [Route("{shortUrl}")]
        public async Task<IActionResult> AddUserToRoom(string shortUrl, [FromBody] string roomUser)
        {
            var clientIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
            if (clientIpAddress != null && myRateLimitService.IsLimitExceeded(clientIpAddress.ToString()))
            {
                return BadRequest($"Limit Exceeded for your IP: {clientIpAddress}");
            }

            if (!IsRequestValid(roomUser, out var badRequest))
            {
                return badRequest;
            }

            var roomData = (await myFirebaseClient.Child(Rooms)
                .Child(shortUrl).OnceAsync<RoomDto>()).SingleOrDefault();
            if (roomData == null)
            {
                return NotFound("Specified room is not available.");
            }

            if (roomData.Object.Users.ContainsKey(roomUser))
            {
                return BadRequest("User already exist in the room.");
            }

            roomData.Object.Users.Add(roomUser, string.Empty);

            await myFirebaseClient.Child(Rooms).Child(shortUrl).Child(roomData.Key).PutAsync(roomData.Object);
            return Ok(roomData.Object);
        }

        [HttpPut]
        [Route("assignChitsToUsers/{shortUrl}")]
        public async Task<IActionResult> AssignChitsToUsers(string shortUrl, [FromBody] IEnumerable<string> chits)
        {
            var clientIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
            if (clientIpAddress != null && myRateLimitService.IsLimitExceeded(clientIpAddress.ToString()))
            {
                return BadRequest($"Limit Exceeded for your IP: {clientIpAddress}");
            }

            var chitList = chits?.ToList();
            if (chitList?.Any(chit => !string.IsNullOrWhiteSpace(chit)) == false)
            {
                return BadRequest("Chits cannot be empty");
            }

            var roomData = (await myFirebaseClient.Child(Rooms)
                .Child(shortUrl).OnceAsync<RoomDto>()).SingleOrDefault();
            if (roomData == null)
            {
                return NotFound("Specified room is not available.");
            }

            if (roomData.Object.Users.Count != chitList.Count)
            {
                return BadRequest($"Number of chits ({chitList.Count}) should be equal to number of users ({roomData.Object.Users.Count}) in room. ");
            }

            AssignChitsToUsers(chitList, roomData.Object.Users);

            await myFirebaseClient.Child(Rooms).Child(shortUrl).Child(roomData.Key).PutAsync(roomData.Object);
            return Ok(roomData.Object);
        }

        private static void AssignChitsToUsers(IList<string> chitList, IDictionary<string, string> users)
        {
            chitList.Shuffle();
            var i = 0;
            foreach (var userPair in users)
            {
                users[userPair.Key] = chitList[i++];
            }
        }

        private bool IsRequestValid(string roomOwner, out IActionResult actionResult)
        {
            actionResult = null;
            if (string.IsNullOrWhiteSpace(roomOwner))
            {
                {
                    actionResult = BadRequest($"Name: '{roomOwner}' cannot be empty.");
                    return false;
                }
            }

            return true;
        }

        private bool IsNextRangeDue()
        {
            if (myRangeService.NextNumberRange == null)
            {
                return true;
            }

            var mid = (myRangeService.NumberRange.End + myRangeService.NumberRange.Start) / 2;
            return myRangeService.NumberRange.Current > mid &&
                   myRangeService.NextNumberRange.Start < mid;
        }

        private static string GetBase62Url(long num)
        {
            var result = new StringBuilder();
            var baseLen = MyAlphanumeric.Length;
            while (num > 0)
            {
                result.Append(MyAlphanumeric[(int) num % baseLen]);
                num /= 10;
            }

            return result.ToString();
        }
    }
}