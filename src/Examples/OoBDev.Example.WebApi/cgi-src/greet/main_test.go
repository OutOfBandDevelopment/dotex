package main

import (
	"encoding/json"
	"net/http/httptest"
	"net/url"
	"strings"
	"testing"
)

func TestGreetShowsFormWithoutName(t *testing.T) {
	rec := httptest.NewRecorder()
	Greet(rec, httptest.NewRequest("GET", "/cgi-bin/greet.cgi", nil))
	if !strings.Contains(rec.Body.String(), "Who are you?") {
		t.Fatalf("expected the form, got %q", rec.Body.String())
	}
}

func TestGreetSendsNameBack(t *testing.T) {
	form := url.Values{"name": {"Ada"}}
	req := httptest.NewRequest("POST", "/cgi-bin/greet.cgi", strings.NewReader(form.Encode()))
	req.Header.Set("Content-Type", "application/x-www-form-urlencoded")
	rec := httptest.NewRecorder()
	Greet(rec, req)
	if !strings.Contains(rec.Body.String(), "Hello, Ada!") {
		t.Fatalf("expected a greeting, got %q", rec.Body.String())
	}
}

func TestGreetEscapesName(t *testing.T) {
	form := url.Values{"name": {"<script>alert(1)</script>"}}
	req := httptest.NewRequest("POST", "/x", strings.NewReader(form.Encode()))
	req.Header.Set("Content-Type", "application/x-www-form-urlencoded")
	rec := httptest.NewRecorder()
	Greet(rec, req)
	if strings.Contains(rec.Body.String(), "<script>") {
		t.Fatalf("name was not escaped: %q", rec.Body.String())
	}
}

func TestOpenAPIDescribesGetAndPost(t *testing.T) {
	var buf strings.Builder
	if err := WriteOpenAPI(&buf); err != nil {
		t.Fatal(err)
	}
	var doc struct {
		Paths map[string]map[string]any `json:"paths"`
	}
	if err := json.Unmarshal([]byte(buf.String()), &doc); err != nil {
		t.Fatalf("not valid JSON: %v", err)
	}
	for _, method := range []string{"get", "post"} {
		if _, ok := doc.Paths["/"][method]; !ok {
			t.Errorf("missing %s on /", method)
		}
	}
}
