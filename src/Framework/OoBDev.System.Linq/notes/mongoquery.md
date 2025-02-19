[
  {
    $addFields: {
    FixedUp: {
      $function:{
          body: function(datecode, offset) {
        	  return  (offset * 60 * 1000) + datecode;
      	  },
          args: [
            { $toLong: [{ $arrayElemAt: ["$createdOn",0] }]},
            { $toLong: { $arrayElemAt: ["$createdOn",1] }}
          ],
          lang: "js"
        }      
      }
    }
  },
  {
    "$project": {
      "UserId": "$_id",
      "UserName": "$userName",
      "EmailAddress": "$emailAddress",
      "FirstName": "$firstName",
      "LastName": "$lastName",
      "Active": "$active",
      "UserModules": {
        "$cond": {
          "if": { "$eq": [ "$userModules", null ] },
          "then": null,
          "else": {
            "$map": {
              "input": "$userModules",
              "as": "module",
              "in": {
                "Code": "$$module.code",
                "Name": "$$module.name",
                "Roles": {
                  "$cond": {
                    "if": { "$eq": [ "$$module.roles", null ] },
                    "then": null,
                    "else": {
                      "$map": {
                        "input": "$$module.roles",
                        "as": "role",
                        "in": {
                          "Code": "$$role.code",
                          "Name": "$$role.name",
                          "Rights": {
                            "$cond": {
                              "if": { "$eq": [ "$$role.rights", null ] },
                              "then": null,
                              "else": {
                                "$map": {
                                  "input": "$$role.rights",
                                  "as": "right",
                                  "in": {
                                    "Name": "$$right.name",
                                    "Code": "$$right.code"
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "CreatedOn": "$createdOn",

      "CreatedOn-Built": {
          $add: [
            { $toLong: {$multiply:[ { $arrayElemAt: ["$createdOn",1] }, 60, 1000 ]}},
            { $toLong: [{ $arrayElemAt: ["$createdOn",0] }]}
          ]
      },
      
      "CreatedOn-FixedUp": "$FixedUp",
      
      "_id": 0
    }
  },
  {
    $match: {
      "CreatedOn-Built": { $gt: 636415154217956000} 
    }
  },
  {
    "$sort": {
      "LastName": 1,
      "FirstName": 1,
      "EmailAddress": 1
    }
  },
  { "$skip": NumberLong(0) },
  { "$limit": NumberLong(10) }
]