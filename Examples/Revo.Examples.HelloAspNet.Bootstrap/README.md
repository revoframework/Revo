# Revo.Examples.HelloAspNet.Bootstrap

This is a sample Hello World application for the Revo framework.
It saves to-do notes and consists of an Web API controller, one command, one query and one aggregate root.

Run the application, browse to the index page and try sending a HTTP request:
 * GET /todos
 * POST /todos {title:string}

How to get started:
 * Create a new MSSQL database.
 * Run these SQL scripts in the newly created database:
     'Infrastructure/Revo.Infrastructure/database/01_REV_CREATE.sql',
     'Infrastructure/Revo.Infrastructure/database/03_REV_INDICES.sql'
	 'Examples/Revo.Examples.HelloAspNet.Bootstrap/database/01_REX_CREATE.sql'.
 * Open Web.config and change the MSSQL database connection string (EFConnectionString) under connectionStrings section.
 * Build the solution.
 * Run and try the application with your browser.