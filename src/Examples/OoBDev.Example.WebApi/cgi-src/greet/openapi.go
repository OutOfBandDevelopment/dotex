package main

import (
	"encoding/json"
	"io"
)

type obj = map[string]any

// OpenAPI describes what Greet answers. Paths are relative to the program: "/" is the program itself.
// The build runs `greet -openapi` and stores the result next to the program as greet.openapi.json,
// which OoBDev.AspNetCore.Cgi.OpenApi lists in the web API's OpenAPI document.
func OpenAPI() obj {
	html := obj{"text/html": obj{"schema": obj{"type": "string"}}}
	return obj{
		"openapi": "3.1.0",
		"info":    obj{"title": "greet", "version": "1.0.0"},
		"paths": obj{
			"/": obj{
				"get": obj{
					"summary": "Shows the name form, or the greeting when ?name= is given",
					"tags":    []string{"CGI greet"},
					"parameters": []obj{
						{"name": "name", "in": "query", "required": false, "schema": obj{"type": "string"}},
					},
					"responses": obj{"200": obj{"description": "An HTML page", "content": html}},
				},
				"post": obj{
					"summary": "Sends the submitted name back in a greeting",
					"tags":    []string{"CGI greet"},
					"requestBody": obj{
						"required": true,
						"content": obj{
							"application/x-www-form-urlencoded": obj{
								"schema": obj{
									"type":       "object",
									"required":   []string{"name"},
									"properties": obj{"name": obj{"type": "string"}},
								},
							},
						},
					},
					"responses": obj{"200": obj{"description": "An HTML page that greets the name", "content": html}},
				},
			},
		},
	}
}

// WriteOpenAPI writes the description as indented JSON.
func WriteOpenAPI(w io.Writer) error {
	enc := json.NewEncoder(w)
	enc.SetIndent("", "  ")
	return enc.Encode(OpenAPI())
}
