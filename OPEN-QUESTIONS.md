# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

- I think for applications the pattern should be more like external services where `.\Applications\{AppName}\{projects}` versus `.\Applications\{projects}`
- We need a review over all .md files in the projects.  there seems to be a lot of out-of-date and possiblity reduant documents.  should be a lower priority after the migrations are complete
- We should have a stack review... some tools like Xslt.cli and TemplateEngine seem like they are somewhat duplicative and can be merged... if template engine can be made to support different pathing engines such as the xpath navigator and host the xslt transforms then xslt.cli can probably go way.  as long as the functionality exists and a document on how to convert the templates/scripts I think this would be a fine plan.  
- we should also review the binary data decoders users of the xslt.cli... it was orginally used to determinsitically generate much of the documentation.  that should beable to be ported to template engine.  might also want to move that generation to mstest entry points instead of scripts wrapped around dotnet tools so it can be more easily be integrated into the pipeline.  the dotnet tools are a fineway too as long as there are extensions in the .props and .targets correctly so maybe a shootout spike if this change hsould be made.
- the image conversion for ghostscript in devterm around pcl, ps should probably be made more generic and used with the document conversion framework.  the hpgl to svg would be handy as well.  
- having example apps that use the ui defintion stuff would be useful.  
- the devices may not need coverage in the example framework... this could be a tbd spike question as these are covered in devterm.

## Open

_(none open)_
