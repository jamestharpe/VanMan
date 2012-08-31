# VanMan

An Azure-based VANity URL MANager. Build it, deploy it (be sure to update the 
StorageConnectionString and DefaultDestination first!), point the DNS for one or more domains to 
it. Any URL encountered by VanMan that is NOT handled will automatically create a record in Azure 
Table Storage with the default destination value. There is no administrative interface, so use a 
tool like Azure Storage Explorer to set the destination URL and other options.

## Options

To use multiple options, just add. For example to preserve the path AND query string use 1+2=3, so 
use 3 as the "Options" value. Another example: Preserve the path but not the query string and use a 
permanant (HTTP 301) redirect by using 1+4=5. Preserve the path, query, AND make the redirect 
permanent by using 1+2+4=7.

### Default = 0
Does a temporary (HTTP 302) redirect to the destination. Any path or querystring data on the source
is discarded.

### PreservePath = 1
Appends source path information to the destination URL. Example:

<table>
	<tr><th>Source</th><th>Destination</th><th>Options</th></tr>
	<tr><td>http://www.oldsite.com</td><td>http://www.newsite.com/oldcontent/</td><td>1</td></tr>
</table>

By visiting http://www.oldsite.com/some/old/article.html, VanMan will redirect to 
http://www.newsite.com/oldcontent/some/old/article.html.

### PreserveQueryString = 2
Appends source query string to the destination URL. Example:

<table>
	<tr><th>Source</th><th>Destination</th><th>Options</th></tr>
	<tr><td>http://www.oldsite.com</td><td>http://www.newsite.com/oldcontent/</td><td>2</td></tr>
</table>

By visiting http://www.oldsite.com/some/old/article.html?foo=bar&biz=baz, VanMan will redirect to 
http://www.newsite.com/oldcontent/?foo=bar&biz=baz. Note that if you wanted to preserve the path as
well, you would set the options value to 3.

### Permanent = 4

Does a permanent (HTTP 301) redirect rather than the default, temporary (HTTP 302) redirect.

## License
VanMan - An Azure-based VANity URL MANager.
Copyright © 2012 Rollins, Inc.
Additional contributors noted in source.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this program.  If not, see http://www.gnu.org/licenses/.