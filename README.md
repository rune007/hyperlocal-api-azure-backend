# hyperlocal-api-azure-backend
WCF/Azure Services API - Created by Rune Hansen - June 2012

YouTube presentation:  
ASP.NET MVC / Azure Services Backend / Spatial Data - By Rune Hansen  
https://www.youtube.com/watch?v=wtJAy3vMc-A

Hyperlocal - User generated news community In the project is being build a user generated news community focused on hyperlocal news for the country of Denmark. The project gives its users two kind of clients to interact with the applications: An ASP.NET MVC website and a Windows Phone application. The project is implemented on the Windows Azure cloud computing platform. The central data source for the clients is a WCF application. The project implements and explores basic Windows Azure concepts such as SQL Azure, Azure Table storage, Azure blob storage, Azure Queues and Azure Worker Roles. The project also explores and make use of SQL Azure spatial data types and methods.  

The API is consumed by website and smartphone app clients listed below:  
https://github.com/rune007/hyperlocal-website  
https://github.com/rune007/hyperlocal-phone-app  

![service-screen-shots-y](https://user-images.githubusercontent.com/5253939/158234518-3ff31b97-b028-4518-be39-9b66300f7235.png)  
![service-screen-shots-l](https://user-images.githubusercontent.com/5253939/158234528-d3a9b01e-30f9-435a-bf21-3156a805215c.png)  
![service-screen-shots-h](https://user-images.githubusercontent.com/5253939/158234531-6e5b97ae-2801-4126-9d82-553092020e97.png)  
![service-screen-shots-a](https://user-images.githubusercontent.com/5253939/158234609-2ac58fb4-0d15-4395-9ef3-72a801dd0e2b.png)  
![service-screen-shots-d](https://user-images.githubusercontent.com/5253939/158234623-967abe35-7b51-466b-a7c6-ff7e763be7e8.png)  
![service-screen-shots-b](https://user-images.githubusercontent.com/5253939/158234725-9b0ab21e-7da8-4fd4-9ef6-933cb0f3f7f1.png)  
![service-screen-shots-i](https://user-images.githubusercontent.com/5253939/158234747-6f436c9a-819c-4fd4-8199-41a42dc2868b.png)  
![service-screen-shots-j](https://user-images.githubusercontent.com/5253939/158234760-9682ded3-3690-4a6c-9e1b-a083c3c725ce.png)  
![service-screen-shots-n](https://user-images.githubusercontent.com/5253939/158234765-011f3314-a79a-459e-a531-9b4b77da6fd1.png)  
![service-screen-shots-s](https://user-images.githubusercontent.com/5253939/158234899-d73f7d42-189f-4a57-bffa-0f450cb4d040.png)  
![service-screen-shots-t](https://user-images.githubusercontent.com/5253939/158234910-9a96dca6-35ae-4d32-bded-c7549db79bf5.png)  
![service-screen-shots-k](https://user-images.githubusercontent.com/5253939/158234926-c15536f6-f181-43a8-9b52-4e4fbb68ffe1.png)  
![service-screen-shots-m](https://user-images.githubusercontent.com/5253939/158234941-4b21a2c3-6468-4c95-9063-f296db0f7359.png)  
![service-screen-shots-o](https://user-images.githubusercontent.com/5253939/158234953-cf59c7ec-6d5f-45ed-b812-b5c8e126790c.png)  
![service-screen-shots-q](https://user-images.githubusercontent.com/5253939/158234965-dad5b2df-7cfc-4177-be8c-65b03f823930.png)   
DAGI dataset  
From The Danish Ministry of the Environment - National Survey and Cadastre, I was able to obtain the DAGI dataset ("Danmarks Administrative Geografiske Inddeling" - The Administrative geographical division of Denmark)  
![website-screen-shots-x](https://user-images.githubusercontent.com/5253939/158235125-486e5a7b-722a-489c-b754-d056b4fdde44.png)  
Reprojection of Spatial Data with FME Desktop  
The datasets which I obtained lived in the Shapefile .shp GIS (Geographic Information System) data format. SQL Azure does not understand Shapefile, so to import the files to SQL I needed a spatial tool which could translate from Shapefile to SQL geometry data type. The tool I used was the FME Desktop (FME - Feature Manipulation Engine) from Safe Software, a tool for working with spatial data.  After having imported the data to SQL Server, there was still some problems because the geographical data lived in the geometry data type. Maps in the geometry data type treats data as living on a flat Earth. So a line from one point to another is just a straight line. Whereas the geography data type treats data as living on a round Earth, it takes into account the curved shape of the Earth, so a line from one point to another is a curved line. The data sets I had obtained where flat Earth datasets, whereas the data I needed to deal with, for example GPS latitude and longitude coordinates lived in a round Earth world. So I needed to re-project my data from geometry data type to geography data type. Again I made use of the FME Desktop to do this task. Now the problem was what geographic coordinate system should I project the data to? There exists virtually thousands of different geographic coordinate systems, from different times in history and different countries and regions use different geographic systems. Well the answer was quite simple, I knew that I needed to work with the same geographical system as Google Maps, because I needed to put spatial data on a map, that was recognized by their GPS latitude and longitude coordinates, and Google map does that. Google maps uses .kml files to store their data, so I got hold of a .kml file and used FME Desktop to inspect what coordinate system Google maps where using, the answer was LL84/WGS84, so I re-projected my spatial data to this format and finally I was able to use the imported spatial data for organizing my data in my applications. Also when we create spatial objects in SQL do we need to inform SQL Server about the coordinate system we are using. SQL Server, though, does not accept the definition LL84/WGS84, instead it makes use of a spatial reference system which is called SRID (Spatial Reference System Identifier). The SRID value which identifies the LL84/WGS84 coordinate system is 4326.  
![service-screen-shots-c](https://user-images.githubusercontent.com/5253939/158250114-f6507b68-1e32-4308-8cc3-b818b09271d9.png)  
Spatial objects  
I am making use of following spatial objects: Point, LineString, Polygon and MultiPolygon. For example my static spatial data covering the whole country of Denmark lives in SQL Azure as a MultiPolygon.  
![service-screen-shots-e](https://user-images.githubusercontent.com/5253939/158235066-e619a901-269b-450b-a4bd-e50ed665de25.png)  
![service-screen-shots-f](https://user-images.githubusercontent.com/5253939/158235078-c27301b8-b28c-4e0d-9837-ca508e1c5640.png)  
![service-screen-shots-p](https://user-images.githubusercontent.com/5253939/158235085-a201e6e1-bc55-4916-962e-0e9c087859bf.png)  
SQL Server Spatial Methods I am employing:  
MakeValidGeographyFromText(), InstanceOf(), Reduce(), STIntersects(), STDistance(), EnvelopeCenter(), STUnion(), STArea(), STBuffer().  
