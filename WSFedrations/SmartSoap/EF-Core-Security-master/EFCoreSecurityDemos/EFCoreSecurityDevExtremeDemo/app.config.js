window.TestApp = $.extend(true, window.TestApp, {
  "config": {
    "layoutSet": "navbar",
    "navigation": [
      {
        "title": "Login",
        "onExecute": "#Login",
        "icon": "user",
        "visible": ko.observable(true)
      },
      {
        "title": "Contacts",
        "onExecute": "#Contacts",
        "icon": "group",
        "visible": ko.observable(false)
      },      
      {
        "title": "Departments",
        "onExecute": "#Departments",
        "icon": "globe",
        "visible": ko.observable(false)
      },
      {
        "title": "Tasks",
        "onExecute": "#Tasks",
        "icon": "doc",
        "visible": ko.observable(false)
      },
      {
        "title": "About",
        "onExecute": "#About",
        "icon": "info",
        "visible": ko.observable(true)
      }
    ]
  }
});