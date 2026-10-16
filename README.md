# POE PART 1

# **THE PURPOSE OF THIS PROJECT**
The purpose of this project is to turn a paper based system into a web based application. this platform will then be able to handle registrations, results and event info properly.

# **THE FUNCTIONALITY OF THE APPLICATION**
To provide a centralised platform where organisers can manage events, categories, registrations and results, while participants can browse events, register for races and view their performance.

# **Execution Instructions**
Install SQL Server Management Studio, and then from there copy the SQL code included in the docs folder. Highlight everything and then execute. 

# **Screenshot of working workflow**
<img width="1343" height="636" alt="image" src="https://github.com/user-attachments/assets/667c6158-4339-4276-9d42-139b0901f5fb" />

# **Referencing**
MDN Web Docs, 2026. HTTP response status codes. [online] Available at: <https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Status> [Accessed 4 September 2026].
W3Schools, 2026. SQL tutorial. [online] Available at: <https://www.w3schools.com/sql/> [Accessed 4 September 2026].

# RaceDay API (Part 2)
A rest api for managing running, walking and cycling events. Organisers create events, categories and results. Participants enrol in events and view their results.

# **How to run the project**
1. Clone the repo an dopen the soloution in visual studio, you will find part 2 on the part 2 branch
2. In 'appsettings.json' check that 'DefaultConnection' points to the SQL server/ localDb instance
3. In package manager console with 'RaceDayApi' as the default project, run 'update-database' to create the database from the migrations
4. Press **F5**. Swagger will the open

# **The two roles**
**Organiser** runs the events. 
- Creates, updates and deletes their own events (name, description, date, location, distance and type: Run, Walk or Cycle).
- Defines categories for their events. 
- Views everyone enrolled in their own events.
- Captures finish time, positions and status for participants after an event.

**Participant** takes part in events
- Views all events and their categories.
- Enrols in an event by choosing a category, receives a unique race number, can enter an event only once, and can cancel an enrolment.
- Views their own enrolments and their own results.

Both roles can register, log in, log out, view and update their own profile, and view events and categories. Organiser functions are blocked for participants, and participant functions are blocked for organisers.

# **CI/CD Workflow**
<img width="1344" height="623" alt="image" src="https://github.com/user-attachments/assets/129da95b-5c31-4e05-9e20-2c15f55c6b56" />

# **YouTube Video Link**
https://youtu.be/IYchtU4dNN8?si=Iaxz02PXrDHSIL9N

# **Referencing**
freecodeCamp.org(28 May 2025) Build REST APIs in .NET 9 – Full Course for Beginners[online video].Available at: youtube.com(Accessed 9 October 2026).
W3Schools (2026) C# Tutorial. Available at: https://www.w3schools.com/cs/index.php (Accessed 14 October 2026). 
