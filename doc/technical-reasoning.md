# Technical reasoning

This document describes reasoning behind different architectural and technical
decisions related to this project. Purpose of this is help with future
refactoring because the technical landscape and the available Azure services
may have changed from the time these decisions where made. This should also
help choosing new implementations I have forgot to think about some aspect
or I haven't known about some available tool etc.

## Requirements

This sections lists overall requirements.

 1. Main function of the software (Image sending) is atomatically activated
    periodically.
 2. Software needs to be able to handle HTTP requests from Discord and from
    users. These should be relatively rare.
    * Discord MAY add a response time requirement.
 1. Software should prefer Managed Identity authentication.
 1. Software should be cheap to run and it shouldn't cost much when it's not
    active.
 1. Secrets should be stored securely.
 1. Local development with containers should be supported.
 1. Infrastructure creation must be automatic with a single command.

## Technical selection

Azure Function App with dotnet-isolated runtime was chosen.

 * Azure
   * Can handle all of the requirements, but mainly because I'm most familiar
   with this cloud service provider.
 * Azure Function App
   * Should be cheap to run.
   * Supports HTTP requests
   * Supports Managed Identity
   * Can be activated periodically (`TimerTrigger`)
   * Supports containers for local development.
* User Assigned Identity
   * User Assigned identity was chosen because it solves the chicken-egg
   -problem related to function app creation. If System Managed Identity would 
   be used, we would need to do multi step initialization with function app,
   storage RBAC, function app site settings, etc.
 * Isolated worker model
   * Chosen instead of In-process model, because In-Process support is ending.
 * Consumption mode for Function App
   * Should be the cheapest option for this kind of workload.
 * App Service plan
   * Chosen instead of Container Apps, because the development experiment with
   container app was still quite poor.
 * Azure Key Vault
   * Is a secure way to store secrets and integrates well with function app.
