using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MercDevs_ej2.Models;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MercDevs_ej2.Controllers
{
    public class DatosfichatecnicasController : Controller
    {
        private readonly MercyDeveloperContext _context;
        private readonly IConfiguration _configuration;

        public DatosfichatecnicasController(MercyDeveloperContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(string email, int fichaTecnicaId)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { status = "error", message = "Correo no proporcionado." });
            }

            // Generar enlace público para la ficha técnica
            var publicUrl = Url.Action("PublicView", "Datosfichatecnicas", new { id = fichaTecnicaId }, protocol: Request.Scheme);

            // Configurar correo electrónico
            var message = new MailMessage
            {
                From = new MailAddress(_configuration["Email:SenderEmail"]),
                Subject = "Ficha Técnica",
                Body = $"Por favor, haga clic en el siguiente enlace para ver su ficha técnica: <a href='{publicUrl}'>Ver Ficha Técnica</a>",
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(email));

            // Configurar cliente SMTP
            var smtpClient = new SmtpClient
            {
                Host = _configuration["Email:SmtpHost"],
                Port = int.Parse(_configuration["Email:SmtpPort"]),
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(
                    _configuration["Email:SmtpUser"],
                    _configuration["Email:SmtpPass"])
            };

            try
            {
                await smtpClient.SendMailAsync(message);
                return Json(new { status = "success", message = "Correo enviado con éxito." });
            }
            catch (SmtpException ex)
            {
                // Manejar el error con más detalles
                return Json(new { status = "error", message = $"Error al enviar el correo: {ex.Message}" });
            }
            catch (Exception ex)
            {
                // Manejar cualquier otro error
                return Json(new { status = "error", message = $"Error inesperado: {ex.Message}" });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> PublicView(int id)
        {
            var datosFichaTecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);

            if (datosFichaTecnica == null)
            {
                return NotFound();
            }

            return View(datosFichaTecnica);
        }

        public async Task<IActionResult> FichaTecnica(int? id)
        {
            // Completar el código para la vista de FICHA TECNICA COMPLETA
            // El ID recibido por este método es el id de Datos de Ficha Tecnica

            var mercydevsEjercicio2Context = await _context.Datosfichatecnicas
                .Where(d => d.IdDatosFichaTecnica == id)
                .Include(d => d.RecepcionEquipo)
                .Include(d => d.Diagnosticosolucions)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync(d => d.RecepcionEquipoId == id);
            return View(mercydevsEjercicio2Context);
        }

        public async Task<IActionResult> Inicio()
        {
            var mercydevsEjercicio2Context = _context.Datosfichatecnicas.Include(d => d.RecepcionEquipo);
            return View(await mercydevsEjercicio2Context.ToListAsync());
        }

        // GET: Datosfichatecnicas
        public async Task<IActionResult> Index(int id)
        {
            var fichaTecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .Include(d => d.Diagnosticosolucions)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync(d => d.RecepcionEquipoId == id);

            if (fichaTecnica == null)
            {
                return RedirectToAction("Create", new { id });
            }

            return View(fichaTecnica);
        }

        // Listar Datos ficha Tecnica por Recepción de Equipos de Cliente: VerDatosFichaTecnicaPorRecepcion
        public async Task<IActionResult> VerDatosFichaTecnicaPorRecepcion(int id)
        {
            var mercydevsEjercicio2Context = _context.Datosfichatecnicas
                .Where(d => d.RecepcionEquipoId == id)
                .Include(d => d.RecepcionEquipo);
            ViewData["IdRecepcionEquipo"] = id;
            return View(await mercydevsEjercicio2Context.ToListAsync());
        }

        // GET: Datosfichatecnicas/Diagnosticosolucionpordatosficha/5
        public async Task<IActionResult> Diagnosticosolucionpordatosficha(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verdiagnostico = await _context.Datosfichatecnicas
                .Include(r => r.Diagnosticosolucions)
                .Include(r => r.RecepcionEquipo)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);
            if (verdiagnostico == null)
            {
                return NotFound();
            }

            return View(verdiagnostico);
        }

        // GET: Datosfichatecnicas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datosfichatecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);
            if (datosfichatecnica == null)
            {
                return NotFound();
            }

            return View(datosfichatecnica);
        }

        // GET: Datosfichatecnicas/Create
        public IActionResult Create(int? id)
        {
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id");
            ViewData["IdRecepcionEquipo"] = id;
            return View();
        }

        // POST: Datosfichatecnicas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("IdDatosFichaTecnica,FechaInicio,FechaFinalizacion,PobservacionesRecomendaciones,Soinstalado,SuiteOfficeInstalada,LectorPdfinstalado,NavegadorWebInstalado,AntivirusInstalado,RecepcionEquipoId")] Datosfichatecnica datosfichatecnica)
        {
            if (datosfichatecnica.FechaInicio != null)
            {
                datosfichatecnica.RecepcionEquipoId = Convert.ToInt32(id);
                _context.Add(datosfichatecnica);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Recepcionequipoes");
            }
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id", datosfichatecnica.RecepcionEquipoId);
            return View(datosfichatecnica);
        }

        // GET: Datosfichatecnicas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datosfichatecnica = await _context.Datosfichatecnicas.FindAsync(id);
            if (datosfichatecnica == null)
            {
                return NotFound();
            }
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id", datosfichatecnica.RecepcionEquipoId);
            return View(datosfichatecnica);
        }

        // POST: Datosfichatecnicas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDatosFichaTecnica,FechaInicio,FechaFinalizacion,PobservacionesRecomendaciones,Soinstalado,SuiteOfficeInstalada,LectorPdfinstalado,NavegadorWebInstalado,AntivirusInstalado,RecepcionEquipoId")] Datosfichatecnica datosfichatecnica)
        {
            if (id != datosfichatecnica.IdDatosFichaTecnica)
            {
                return NotFound();
            }

            if (datosfichatecnica.FechaInicio != null)
            {
                try
                {
                    _context.Update(datosfichatecnica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DatosfichatecnicaExists(datosfichatecnica.IdDatosFichaTecnica))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Recepcionequipoes");
            }
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id", datosfichatecnica.RecepcionEquipoId);
            return View(datosfichatecnica);
        }

        // GET: Datosfichatecnicas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datosfichatecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);
            if (datosfichatecnica == null)
            {
                return NotFound();
            }

            return View(datosfichatecnica);
        }

        // POST: Datosfichatecnicas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var datosfichatecnica = await _context.Datosfichatecnicas.FindAsync(id);
            _context.Datosfichatecnicas.Remove(datosfichatecnica);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DatosfichatecnicaExists(int id)
        {
            return _context.Datosfichatecnicas.Any(e => e.IdDatosFichaTecnica == id);
        }
    }
}
