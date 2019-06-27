﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EFCoreSecurityODataService {
    public class BasicAuthModule : IHttpModule {
        public void Init(HttpApplication app) {
            app.AuthenticateRequest += new EventHandler(app_AuthenticateRequest);
        }
        private void app_AuthenticateRequest(object sender, EventArgs args) {
            WebApiApplication app = (WebApiApplication)sender;
            if(app.Request.HttpMethod != "OPTIONS") {
                if(!app.Request.Headers.AllKeys.Contains("Authorization")) {
                    CreateNotAuthorizedResponse(app, 401, 1,
                        "Please provide Authorization headers with your request.");
                }
                else if(!BasicAuthProvider.Authenticate(app)) {
                    CreateNotAuthorizedResponse(app, 401, 3, "Logon failed.");
                }
            }
        }
        public static void CreateNotAuthorizedResponse(HttpApplication app, int code, int subCode, string description) {
            HttpResponse response = app.Context.Response;
            response.StatusCode = code;
            response.SubStatusCode = subCode;
            response.StatusDescription = description;
            response.AppendHeader("WWW-Authenticate", "Basic");
            app.CompleteRequest();
        }
        public void Dispose() { }
    }
}