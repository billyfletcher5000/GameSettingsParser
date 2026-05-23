# Game Settings Parser

An application for automatically parsing screenshots of settings pages in games
and outputting in different formats for display.

Uses OCR (via Tesseract) and ML font similarity detection to read text from images,
with an editor for configuring areas of sample images and defining of relationships between
those areas.

Currently supported output types:
* HTML
* Confluence (append/overwrite existing page only)
* Debug .TXT file

Currently the above require changing the used `IAnalysisExportService` in `App.xaml.cs`,
configuration menu in planned updates. Defaults to Confluence.

## Usage



## Development

### Confluence Setup ###

If you are building from source you will need to set up your own Atlassian confluence
app on the [Atlassian Developer Site](https://developer.atlassian.com/) and then set
both the correct scopes and subsequently, the relevant environment variables on your machine 
(if using the default `EnvironmentVariableVaultService`).

**Required Atlassian Scopes:**

`read:content-details:confluence`
`read:page:confluence`
`write:page:confluence`
`write:attachment:confluence`
`read:space:confluence`

**Set environment variables:**

   **Windows (CMD):**
   ```powershell
   setx CONFLUENCE_CLIENT_ID your-client-id
   setx CONFLUENCE_CLIENT_SECRET your-client-secret
   ```

   **Linux/macOS:**
   ```bash
   export CONFLUENCE_CLIENT_ID="your-client-id"
   export CONFLUENCE_CLIENT_SECRET="your-client-secret"
   ```

