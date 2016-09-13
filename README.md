# ArcGIS Server Custom Provider
Custom Provider for ArcGIS Server

# Requirements

ArcGIS Server 10.4 or superior

# Description

Store ArcGIS Server users and roles in a SQL Server security store


# Installation

- Install the custom provider .dll into the GAC.

>gacutil /i ArcGisServerCustomProvider.dll

- Create a db in SQL Server/Express and run ArcGISServerCustomProvider.sql in folder support. Change in first row the name of your db

- Open the ArcGIS Server Administrator Directory and log in with a user who has administrative permissions to your site.
The Administrator Directory is typically available at http://gisserver.domain.com:6080/arcgis/admin. 
Click security > config > updateIdentityStore.
Copy and paste the following text into the User Store Configuration dialog box on the Operation - updateIdentityStore page.

```<language>
{"type": "ASP_NET",
    "class": "ArcGisServerCustomProvider.ArcGisServerMembershipProvider,ArcGisServerCustomProvider,Version=2.0.0.0,Culture=Neutral,PublicKeyToken=e70ef8c9eb62a069",
    "properties": {
        "connectionStringName": "Data Source=.\\SQLEXPRESS;Initial Catalog=;User Id=;Password=;"
    }
}
```

Update the user, password, name database and datasource values in property connectionStringName.
Copy and paste the following text into the Role Store Configuration dialog box on the Operation - updateIdentityStore page.

```<language>
{
  "type": "ASP_NET",
  "class": "ArcGisServerCustomProvider.ArcGisServerRoleProvider,ArcGisServerCustomProvider,Version=2.0.0.0,Culture=Neutral,PublicKeyToken=e70ef8c9eb62a069",
  "properties": {
    "connectionStringName": "Data Source=.\\SQLEXPRESS;Initial Catalog=;User Id=;Password=;"
  }
}
```

Update the user, password, name database and datasource values in property connectionStringName.

- Click Update to save your configuration.



