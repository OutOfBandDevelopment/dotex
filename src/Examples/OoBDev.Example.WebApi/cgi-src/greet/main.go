// Command greet is a small CGI program for the OoBDev.AspNetCore.Cgi example.
// GET shows a form; POST (or GET with ?name=) reads the "name" field and sends it back.
// `greet -openapi` prints its OpenAPI description instead of serving a request.
package main

import (
	"fmt"
	"html"
	"net/http"
	"net/http/cgi"
	"os"
)

const page = `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><title>Greet</title></head>
<body>
<h1>%s</h1>
<form method="post" action="%s">
  <label>Name <input name="name" value="%s" autofocus></label>
  <button type="submit">Send</button>
</form>
</body></html>
`

// Greet answers every request with the form, and with a greeting when a name was sent.
func Greet(w http.ResponseWriter, r *http.Request) {
	name := r.FormValue("name")
	title := "Who are you?"
	if name != "" {
		title = "Hello, " + html.EscapeString(name) + "!"
	}
	w.Header().Set("Content-Type", "text/html; charset=utf-8")
	fmt.Fprintf(w, page, title, html.EscapeString(r.URL.Path), html.EscapeString(name))
}

func main() {
	if len(os.Args) > 1 && os.Args[1] == "-openapi" {
		if err := WriteOpenAPI(os.Stdout); err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(1)
		}
		return
	}
	if err := cgi.Serve(http.HandlerFunc(Greet)); err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
}
