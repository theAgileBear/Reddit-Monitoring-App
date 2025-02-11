# Reddit-Monitoring-App

## Overview
The Reddit Monitoring App is a .NET-based console application designed to monitor a specific subreddit for new posts. It leverages the Reddit API to fetch posts and track statistics such as the top posts based on upvotes and the most active users based on post frequency.

## Features
- Subreddit Monitoring: Continuously monitors a specified subreddit for new posts.
- Statistics Tracking: Tracks and displays the top posts by upvotes and the most active users by post count.
- Rate Limit Handling: Manages Reddit API rate limits by adjusting request intervals dynamically.
- Error Handling: Handles various HTTP response codes and network-related exceptions gracefully.
- Cancellation token used to cancel monitoring services without ending application

## Prerequisities
- .NET 8 SDK
- Reddit Account
- [Reddit Application](https://www.reddit.com/prefs/apps) (Optional)
  - Set to installed app

## Installation
1. Clone Repository
3. Build Application
4. Run Application

## How to use this Application
1. Update config.env file to configurate the reddit API (Optional if created Reddit application above)
   - Client ID
   - Redirect URI
   - User Agent
   - Access Token (Optional)
2. Update program.cs to change
   - Subreddit (default r/funny)
     > r/ not required in string
   - Request Limit
3. Run application
4. Application will open up default browser requiring user to authorize this application to use your reddit account (Press OK in browser)
5. Copy redirected Uri into the browser to give application your access token
6. Application will show output on Console



## Limitations
- Currently only monitors 1 subreddit at a time
- No retry mechanism for server related failures
- Temporary bearer token expires after 1 hour, no refresh token logic implemented
- No data persisting
  - Could store information in a JSON txt file
  - Could create small SQL database to store posts and relative information
- Subreddit and request limits are not in environment file
 
## Packages used
- MSTest.TestFramework
- MSTest.TestAdapter
- System.Net.Http
- Newtonsoft.Json
- Moq
