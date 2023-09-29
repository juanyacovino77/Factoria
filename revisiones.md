_Para que escale mejor, separar las clases de contratos en 1 archivo por clase (convencion NET)_
Cumplir con convencion de nombres (variables, metodos, etc, hay varias estilos distintos)

Seguridad: Como se aplica en la aplicacion?? como asegurar que las invocaciones a metodos controlan autenticacion y autorizacion?

Forma del documento:
Cada titulo podria arrancar en una hoja diferente (ejemplo: objetivo general)

En la descripcion del problema trataria de poner valores cuantitativos de las metricas que se ven afectadas y dan pie a la creacion del software (“genera perdidas”, podemos calcular cuanto se pierde?? aca es importante ademas del numero explicar de donde sale ese numero, como se calcula, para demostrar q no es un valor antojadizo.

_Naturalmente todo el tiempo surgen imprevistos, urgencias, nuevas promociones, cambios en los precios y se necesita distribuir la información lo más rápido posible para que los trabajadores de todos los sectores actúen cuanto antes_
Evitaria el uso de definiciones ambiguas como “todo el tiempo”, definiria bien cuales son los imprevistos, de que tipo son, en que % suceden entre ellos y como afecta en detalle a la operacion del negocio.

_La rotación del personal es bastante frecuente, esto hace que se pierda cierto conocimiento de negocio del sector_
Que numero le ponemos a “bastante frecuente”?

_Implementar un software servidor que corra en Windows o Linux que expondría_
Evitaria los potenciales, que exponga. 

En antecedentes, agregaria que todo proceso manual es propenso a errores involuntarios o voluntarios (fraude) y que eso tambien es resuelto en parte por el sistema que estas haciendo. Fraude seria si todos los empleados se ponen de acuerdo para llevar 1kg de milanesas por dia anotando mal en la planilla.
Esto, aunque pueda no pasar en esta empresa, podria pasar en otra, hay que pensar el sistema para el tipo de problema que quiere resolver abstrayendo el caso concreto.

_La empresa Iacoyá SRL satisface los servicios de muchos clientes desde 1990_
satisface las necesidades o brinda servicios, no satisface servicios

_Esta es una filosofía con algunos años, pero recién empieza a consolidarse_
Ojo con estas cosas, DDD como concepto y auge viene desde los 2000 y hace rato que se usa para los casos de uso en donde aplica mejor

_Tradicionalmente a la hora de pensar una aplicación, el ingeniero se ponía a pensar en un MODELO DE DATOS y como se traduciría esas relaciones a objetos en memoria RAM, respondiendo más bien a una programación funcional_
Esto esta definitivamente mal, no tiene nada que ver prog funcional con modelo de datos y objetos en memoria (prog funcional no es orientado a objetos por ejemplo)

_En una arquitectura de capas mal implementadas (como en general siempre se hizo), la capa del Dominio, la lógica de negocios o modelo de objetos tenía una referencia directa a la capa de Datos y desde ahí se mandaban los comandos para persistir los objetos transcientes_
Estas afirmaciones tan globalistas se convierten en dogmas, en nuestra carrera nada es absoluto, la arquitectura como la nombras resuelve muy bien casos de uso particulares en donde funciona mejor, este es el modelo que usa Wordpress por ejemplo y tiene millones de websites en todo el mundo.
Es un error bastante grave el hecho de ponderar una tecnologia “buena” sobre otra “mala”, podes compararlas y explicar por que en este caso de uso la tuya es la que se debe usar pero no caer en juicios de valor subjetivos.

Algo importante que falta es la parte de estado del arte, buscar aplicaciones que hagan lo mismo que vos queres, y evaluarlas, y concluir si no se pueden usar definiendo que les falta y porq tenes que hacer una nueva. Incluso pernsaria, porq no slack con google drive? cosas de ese estilo que indiquen un analisis

