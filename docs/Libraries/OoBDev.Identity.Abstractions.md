# OoBDev.Identity.Abstractions


## Class: Identity.IIdentityManager
Represents an identity manager for managing user identities. 

### Methods


#### GetIdentityUsersByEmail(System.String)
Retrieves a list of user identity models based on the specified email address. 


##### Parameters
* *emailAddress:* The email address to search for.




##### Return value
A task that represents the asynchronous operation. The task result contains the list of user identity models or null if no users are found.



#### CreateIdentityUserAsync(System.String,System.String,System.String)
Creates a new identity user asynchronously with the specified details. 


##### Parameters
* *email:* The email address of the user.
* *firstName:* The first name of the user.
* *lastName:* The last name of the user.




##### Return value
A task that represents the asynchronous operation. The task result contains the object ID and an optional password for the created user.



#### RemoveIdentityUserAsync(System.String)
Removes an identity user asynchronously based on the specified object ID. 


##### Parameters
* *userId:* The object ID of the user to remove.




##### Return value
A task that represents the asynchronous operation. The task result is true if the user was successfully removed; otherwise, false.



## Class: Identity.IUserManagementProvider
Provides methods for managing user accounts. 

### Methods


#### CreateAccountAsync(OoBDev.Identity.UserCreateModel)
Creates a user account asynchronously based on the provided model. 


##### Parameters
* *model:* The model containing user account information.




##### Return value
A task representing the asynchronous operation, containing the created user model.



## Class: Identity.UserCreatedModel
Represents the model for a user created as a result of account creation. 

### Properties

#### Username
Gets or sets the username associated with the created user.
#### Password
Gets or sets the password associated with the created user.

## Class: Identity.UserCreateModel
Represents a model for creating a user in Microsoft B2C Identity. 

### Properties

#### FirstName
Gets or sets the first name of the user.
#### LastName
Gets or sets the last name of the user.
#### EmailAddress
Gets or sets the email address of the user.

## Class: Identity.UserIdentityModel
Represents a model for user identity information. 

### Properties

#### FirstName
Gets or sets the first name of the user.
#### LastName
Gets or sets the last name of the user.
#### EmailAddress
Gets or sets the email address of the user.
#### UserName
Gets or sets the username of the user.
#### Password
Gets or sets the password of the user.
#### ForcePasswordChangeNextSignIn
Gets or sets a value indicating whether the user should be forced to change their password at the next sign-in.