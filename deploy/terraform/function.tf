terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.0.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = "DisneyWaitTimes"
  location = "WestEurope"
}

resource "azurerm_storage_account" "sa" {
  name                     = "disneywaittimesstorage"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "wait_times" {
  name                  = "wait-times"
  storage_account_name  = azurerm_storage_account.sa.name
  container_access_type = "blob"
}

resource "azurerm_storage_container" "series" {
  name                  = "series"
  storage_account_name  = azurerm_storage_account.sa.name
  container_access_type = "blob"
}

resource "azurerm_application_insights" "insights" {
  name                = "DisneyWaitTimesInsights"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
}

resource "azurerm_service_plan" "plan" {
  name                = "DisneyWaitTimesServicePlan"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "Y1"
}

resource "azurerm_linux_function_app" "fnc" {
  name                = "DisneyWaitTimesFunctionApp"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.insights.instrumentation_key
  }

  lifecycle {
    ignore_changes = [ app_settings ]
  }

  storage_account_name       = azurerm_storage_account.sa.name
  storage_account_access_key = azurerm_storage_account.sa.primary_access_key
  service_plan_id            = azurerm_service_plan.plan.id

  site_config {}
}
