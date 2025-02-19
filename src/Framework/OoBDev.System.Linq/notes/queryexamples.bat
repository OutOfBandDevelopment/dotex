

REM note... ^ is escape character

ECHO unfiltered
curl https://localhost:7192/api/users


ECHO Matches 'Allen'
curl https://localhost:7192/api/users?SearchTerm=Allen

ECHO Starts with 'Ji'
curl https://localhost:7192/api/users?SearchTerm=Ji*

ECHO Starts with 'a' and shows second page
curl https://localhost:7192/api/users?CurrentPage=1^&SearchTerm=a*

ECHO shows second page with changed page length
curl https://localhost:7192/api/users?CurrentPage=1^&PageSize=20

ECHO Starts with 'Ji'
curl https://localhost:7192/api/users?SearchTerm=Ji*
