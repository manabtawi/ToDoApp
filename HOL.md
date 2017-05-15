# Part 1: Create the Main Solution
- From Azure portal create and deploy **Web App + SQL** or you can create both of them separately.
- From Visual Studio 2017 create new solution `ASP.NET Web Application .NET Framework` and name it `ToDoApp`.
- Choose **Web API** from the template box.
- Change the Authetication to **Individual Account User Accounts**
- On Visual Studio navigate to **SQL Server Object Explorer** and click **Add SQL Server**, then expand *Azure* in order to connect to your database that created using Azure Portal.
- Right click on the database and select **properties** then copy the **connection string** attribute value.
- Paste the copied **connection string** in the `Web.config` in the `DefaultConnection` and make sure to replace the password from stars `;Password=******;` to the actual password that has been set prior the deployment.
 
 Example:
 ```
 <add name="DefaultConnection" connectionString="Data Source=SERVER_NAME.database.windows.net;Initial Catalog=DB_NAME;Integrated Security=False;User ID=DB_USERNAME;Password=DB_PASSWORD;Connect Timeout=15;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False" providerName="System.Data.SqlClient" />
 ```
 - Press **F5** to build and run the web app.
 
 # Part 2: Configure the SMS API
 - Right click on the solution and **Add New Project** and select **Class Library (.NET Framework)**, then name it to `ToDoApp.REST.SMS`.
 - Right click on the project `ToDoApp.REST.SMS` then add **REST API Client**.
 - In the Add *REST API Client* window  set the **Swagger URL** to `https://jotechiesapi.azurewebsites.net/swagger/docs/v1` and change the *Namespace* to `SMSAPI`. 
*Note: The above swagger URL is an API interface deoplyed to **Azure API App** and implements **[Twilio](http://www.twilio.com) SDK***
 - Build the project **ToDoApp.REST.SMS**.
 - In the **ToDoApp** project right click on *References* the *Add Reference*, check on *ToDoApp.REST.SMS* then click OK.
 - Create class `SmsModel.cs` in the model folder and place the following snippet:
 ```
 public class SmsModel
    {
        public string from { get; set; }
        public string to { get; set; }
        public string body { get; set; }
    }
 ```
 Make sure the class name is `SmsModel` NOT `SmsModels`
 - Right click on the project and click *Manage NuGet Packages* and install package `Microsoft.Rest.ClientRuntime`.
 - Create folder named `Utils`
 - Under **Utils** folder create class `SmsHelper.cs` and place the following snippet:
 ```
 static public void SendShortCode(ToDoApp.Models.SmsModel model)
        {
            using (SMSAPIClient client = GetSMSAPIClient())
            {
                client.Create(new SMSAPI.Models.SmsModel(Settings.SMS_API_SENDER, "+" + model.to, model.body));
            };
        }

        public static SMSAPIClient GetSMSAPIClient()
        {
            ServiceClientCredentials credentials = new TokenCredentials("<bearer token>");
            var client = new SMSAPIClient(new Uri(Settings.SMS_API_ENDPOINT), credentials);
            return client;
        }

        static public string GetRandomCode()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }
 ```
 And add the following directives:
 ```
using System.Net.Http;
using System.Threading.Tasks;
using SMSAPI;
using Microsoft.Rest;
 ```
 - Under **Utils** folder create class `Settings.cs` and place the following snippet under the `Settings` class:
 ```
public static string SMS_API_ENDPOINT = ConfigurationManager.AppSettings["SMS_API_ENDPOINT"];
public static string SMS_API_SENDER = ConfigurationManager.AppSettings["SMS_API_SENDER"];
 ```
 And add the following directive: `using System.Configuration;`
 - Go to `Web.config` and place the following under `<appSetting>`:
 ```
 <add key="SMS_API_ENDPOINT" value="https://jotechiesapi.azurewebsites.net" />
 <add key="SMS_API_SENDER" value="+19094747918" />
 ```
 - Build the project.
 
 # Part 3: Configure the Account API
 - In the `AccountController.cs` go to the `Register` method replace its code with the followig snippet:
 ```
 if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityUser identity = UserManager.FindByName(model.UserName);
            model.Password = Utils.SmsHelper.GetRandomCode();

            if (identity == null)
            {
                var user = new ApplicationUser() { UserName = model.UserName };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }
            }
            else
            {
                string resetToken = await UserManager.GeneratePasswordResetTokenAsync(identity.Id);
                IdentityResult result = await UserManager.ResetPasswordAsync(identity.Id, resetToken, model.Password);
                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }
            }
            

            // Send SMS short code
            Utils.SmsHelper.SendShortCode(new SmsModel()
            {
                to = model.UserName,
                body = model.Password
            });                           

            return Ok();
 ```
 - Navigate to `AccountBindingModels.cs` under **Models** folder and replace the `RegisterBindingModel` class with the following:
 ```
  public class RegisterBindingModel
    {
        [Required]
        [RegularExpression("^9627[0-9]{8}$", ErrorMessage = "Invalid mobile number format")]
        public string UserName { get; set; }
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
 ```
 - In the `IdentityConfig.cs` under folder **App_Start** set the following flags values:
 
 In the *UserValidator*: `RequireUniqueEmail = false`
 
 In the *PasswordValidator*:
 ```
RequiredLength = 4,
RequireNonLetterOrDigit = false,
RequireDigit = true,
RequireLowercase = false,
RequireUppercase = false,
 ```
 - Press **F5** to build and run the web app.
 - From REST client tool create the following requests:
   - Register Account

```
POST /api/account/register
  
Host: localhost:54112 (Project URL)
  
Content-Type: application/json
  
Request Body: {"username" : "MOBILE_NUMBER_HERE"}
```
*You should receive the short code as SMS to the given mobile number.*
  - Generate Access Token
```
POST /api/token
  
Host: localhost:54112 (Project URL)
  
Content-Type: application/x-www-form-urlencoded
  
Request Body: username=MOBILE_NUMBER_HERE&password=SHORT_CODE&grant_type=password
```
  *You should get the following response example:*
```
{
    "access_token": "ACCESS_TOKEN_HERE",
    "token_type": "bearer",
    "expires_in": 1209599,
    "userName": "MOBILE_NUMBER",
    ".issued": "Mon, 15 May 2017 09:05:54 GMT",
    ".expires": "Mon, 29 May 2017 09:05:54 GMT"
}
```

Now the app is ready for mobile number registration and confirmation through short code SMS. Let's create the ToDo secured APis in the next part.

# Part 4: Create Secured ToDo APIs
- In the **SQL Server Object Explorer** go to the database and execute the following SQL statement in order to create the *ToDo* table:
```
CREATE TABLE [dbo].[ToDo] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (MAX) NOT NULL,
    [Created]  DATETIME       NULL,
    [Modified] DATETIME       NULL,
    [UserId]   NVARCHAR (128) NOT NULL,
    [Notes]    NTEXT          NULL,
    [Done]     BIT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
```
- Right-click on the solution and **Add New Project** and select **Class Library (.NET Framework)**, then name it to `ToDoApp.EF`.
- Right-click on the project *ToDoApp.EF* and add new item *ADO.NET Entity Data Model* and name it `ToDoModel`.
- Select *EF Designer from database* then click next.
- Choose "*Yes, include the sensitive data in the connection string*".
- Make sure to tick the check box "*Save connection setting in App.Config as: *" `ToDoEntities`, then click next.
- Select table *ToDO* from tables.
- Rename the *Model Namespace to* `ToDoModel`, then click finish.
- Copy the connection string setting `ToDoEntities` from *App.Config* in the *ToDoApp.EF* project and place it in the *Web.Config* of the *ToDoApp* project.
- In the *ToDoApp* project:
  - Create new model `ToDoModels.cs` and add the following class:
  ```
  public class ToDoItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public string UserId { get; set; }
        public string Notes { get; set; }
        public bool? Done { get; set; }
    }
  ```
  - Create new controller by right-click on the *Controllers*, then select `Web API 2 Controller - Empty` and name it `ToDoController`.
  - Place the following code snippet:
  ```
  private ToDoEntities db = new ToDoEntities();
        private string userId = string.Empty;

        // GET: api/ToDo
        public IEnumerable<ToDoItem> Get()
        {
            List<ToDoItem> rows = new List<ToDoItem>();
            userId = RequestContext.Principal.Identity.GetUserId();
            foreach (ToDo row in db.ToDoes.Where(x => x.UserId == userId))
            {
                ToDoItem obj = new ToDoItem()
                {
                    Id = row.Id,
                    Name = row.Name,
                    Created = row.Created,
                    Modified = row.Modified,
                    UserId = row.UserId,
                    Notes = row.Notes,
                    Done = row.Done
                };
                rows.Add(obj);
            }
            return rows;
        }

        // GET: api/ToDo/5
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> Get(int id)
        {
            userId = RequestContext.Principal.Identity.GetUserId();
            ToDo row = await db.ToDoes.FindAsync(id);
            if (row == null)
            {
                return NotFound();
            }
            if (row.UserId != userId)
            {
                return Unauthorized();
            }
            return Ok(new ToDoItem()
            {
                Id = row.Id,
                Name = row.Name,
                Created = row.Created,
                Modified = row.Modified,
                UserId = row.UserId,
                Notes = row.Notes,
                Done = row.Done
            });
        }

        // POST: api/ToDo
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> Post(ToDoItem model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ToDo obj = new ToDo()
            {
                Name = model.Name,
                Created = DateTime.Now,
                UserId = RequestContext.Principal.Identity.GetUserId(),
                Notes = model.Notes,
                Done = model.Done
            };
            db.ToDoes.Add(obj);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = obj.Id }, obj);
        }

        // PUT: api/ToDo/5
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> Put(int id, ToDoItem model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            userId = RequestContext.Principal.Identity.GetUserId();


            ToDo obj = new ToDo()
            {
                Id = id,
                Name = model.Name,
                Modified = DateTime.Now,
                UserId = userId,
                Notes = model.Notes,
                Done = model.Done
            };

            db.Entry(obj).State = EntityState.Modified;

            if (db.ToDoes.Where(x => x.Id == id && x.UserId == userId).SingleOrDefault() == null)
            {
                return Unauthorized();
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/ToDo/5
        [ResponseType(typeof(ToDoItem))]
        public async Task<IHttpActionResult> Delete(int id)
        {
            userId = RequestContext.Principal.Identity.GetUserId();
            ToDo row = await db.ToDoes.FindAsync(id);
            if (row == null)
            {
                return NotFound();
            }
            if (row.UserId != userId)
            {
                return Unauthorized();
            }
            db.ToDoes.Remove(row);
            await db.SaveChangesAsync();

            return Ok(row);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ItemExists(int id)
        {
            return db.ToDoes.Count(e => e.Id == id) > 0;
        }
  ```
  - Add the following directives to the current ones:
  ```
  using ToDoApp.EF;
  using ToDoApp.Models;
  using System.Data;
  using System.Data.Entity;
  using System.Data.Entity.Infrastructure;
  using Microsoft.AspNet.Identity;
  using System.Threading.Tasks;
  using System.Web.Http.Description;
  ```
  - At the top of the controller name add the attribute `[Authorize]` in order to secure all operations. It will looks like:
  ```
    [Authorize]
    public class ToDoController : ApiController
    {
    ....
  ```
  - Build the project.
  
  # Part 5: Testing
  We have create 5 operations under ToDO API. Each request shall contain the authorization token in its header in order to be identified and able to access.
  
  On Visual Studio Press F5 and using the REST client tool test the following operations:
  
  **List of Operations**
  - Get All Items
  ```
  GET /api/todo

  Host: localhost:54112 (Project URL)

  Authorization: Bearer TOKEN_HERE
  ```
  - Get Item
  ```
  GET /api/todo/ITEM_ID_HERE
  
  Host: localhost:54112 (Project URL)

  Authorization: Bearer TOKEN_HERE
  ```
  - Create Item
  ```
  POST /api/todo/
  
  Host: localhost:54112 (Project URL)

  Authorization: Bearer TOKEN_HERE
  
  Request body: 
  {
    "Name": "TASK NAME",
    "Notes": "TASK NOTES",
  }
  ```
  - Delete Item
  ```
  DELETE /api/todo/ITEM_ID_HERE
  
  Host: localhost:54112 (Project URL)

  Authorization: Bearer TOKEN_HERE
  ```
  - Update Item
  ```
  PUT /api/todo/ITEM_ID_HERE
  
  Host: localhost:54112 (Project URL)

  Authorization: Bearer TOKEN_HERE
  
  Request body:
  {
    "Id": ITEM_ID_HERE,
    "Name": "TASK_NAME",
    "Notes": "TASK_NOTES",
    "Done": 0 (Boolean: 0 OR 1)
  }
  ```
  
  ## Part 6: Publish you API
  - Right+click on `ToDoApp` project and click *Publish*
  - Select *Microsoft Azure App Service* and choose select existing.
  - Click Publish. You might be asked to select the App service needed for publishing.
  - Click OK then the App will begin publishing itself to the server. The DB configuration will be automatically configured from the *Web.Config*
  - From *Azure Portal* got to *Applications settings* and set the app settings that have been set in the local *Web.Config*.
  - Test your APIs as well as the previous part by changing to the new endpoint.
